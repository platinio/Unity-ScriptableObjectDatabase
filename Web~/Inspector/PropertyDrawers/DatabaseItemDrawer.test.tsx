import { createRendererRegistry, PropertyForm, type PropertyNode } from '@hefesto/form';
import {
  rendererRegistrationsOf,
  UnityQuestionsProvider,
  type UnityQuestions,
} from '@hefesto/inspector';
import type { AssetInfo, UnityAsset } from '@hefesto/unity';
import { builtInWidgets } from '@hefesto/widgets';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import plugin from '../plugin.ts';

const DATABASE_TYPE = 'ArcaneOnyx.Damage.DamageTypeDatabase';
const registry = createRendererRegistry([...builtInWidgets, ...rendererRegistrationsOf([plugin])]);

const databases: UnityAsset[] = [
  {
    guid: 'db1',
    name: 'Damage Types',
    path: 'Assets/Damage Types.asset',
    type: 'DamageTypeDatabase',
  },
  {
    guid: 'db2',
    name: 'Special Damage Types',
    path: 'Assets/Special.asset',
    type: 'SpecialDamageTypeDatabase',
  },
];

function itemOf(localId: string, name: string, fields: Record<string, unknown>) {
  return {
    name,
    type: 'DamageType',
    typeName: 'ArcaneOnyx.Damage.DamageType',
    localId,
    id: `GlobalObjectId_V1-2-db1-${localId}-0`,
    fields,
  };
}

const infos: Record<string, AssetInfo> = {
  db2: {
    guid: 'db2',
    path: 'Assets/Special.asset',
    type: 'SpecialDamageTypeDatabase',
    subAssets: [itemOf('30', 'Holy', { Name: 'Holy' })],
  },
  db1: {
    guid: 'db1',
    path: 'Assets/Damage Types.asset',
    type: 'DamageTypeDatabase',
    subAssets: [
      itemOf('20', 'physical_item', {
        Name: 'Physical',
        icon: { guid: 'icons', localId: '5', name: 'sword', typeName: 'Sprite' },
      }),
      itemOf('10', 'Fire', { Name: '' }),
    ],
  },
};

const NOT_CONNECTED = { connected: false, playing: false, compiling: false };

function fakeUnity(overrides: Partial<UnityQuestions> = {}) {
  const unity = {
    searchAssets: vi.fn(async () => databases),
    assetInfo: vi.fn(
      async (guid: string) => infos[guid] ?? { guid, path: '', type: '', subAssets: [] },
    ),
    derivedTypes: vi.fn(),
    readObject: vi.fn(),
    openAsset: vi.fn(),
    bridge: {
      subscribeStatus: () => () => undefined,
      getStatus: () => NOT_CONNECTED,
      request: vi.fn(),
    },
    ...overrides,
  };
  return unity as unknown as UnityQuestions & typeof unity;
}

function damageTypeField(
  path: string,
  value: unknown,
  overrides: Partial<PropertyNode> = {},
): PropertyNode {
  return {
    path,
    label: path,
    kind: 'reference',
    value,
    hints: {
      attributes: ['ScriptableItemDatabaseSelector'],
      attributeValues: { ScriptableItemDatabaseSelector: { DatabaseType: DATABASE_TYPE } },
    },
    ...overrides,
  };
}

function draw(unity: UnityQuestions, ...fields: PropertyNode[]) {
  const onChange = vi.fn();
  render(
    <UnityQuestionsProvider value={unity}>
      <PropertyForm
        tree={{ path: '', label: 'Asset', kind: 'object', value: null, children: fields }}
        onChange={onChange}
        registry={registry}
      />
    </UnityQuestionsProvider>,
  );
  return onChange;
}

const PHYSICAL = { guid: 'db1', localId: '20', name: 'physical_item', typeName: 'DamageType' };

async function openPicker(label = 'type') {
  await userEvent.click(screen.getByRole('button', { name: `Pick ${label}` }));
  return within(await screen.findByRole('list', { name: `Items of ${DATABASE_TYPE}` }));
}

describe('ScriptableItemDatabaseSelector', () => {
  it("shows the chosen item's name, and None when there is none", () => {
    draw(fakeUnity(), damageTypeField('type', PHYSICAL), damageTypeField('other', null));

    expect(screen.getByRole('button', { name: 'Pick type' })).toHaveTextContent('physical_item');
    expect(screen.getByRole('button', { name: 'Pick other' })).toHaveTextContent('None');
  });

  it('lists None, then the items of databases of exactly that type, by their Name field', async () => {
    draw(fakeUnity(), damageTypeField('type', null));

    const list = await openPicker();

    await waitFor(() =>
      expect(list.getAllByRole('button').map((button) => button.lastChild?.textContent)).toEqual([
        'None',
        'Fire',
        'Physical',
      ]),
    );
  });

  it('sends the picked item as its file and its place in the file, and None as null', async () => {
    const onChange = draw(fakeUnity(), damageTypeField('type', null));

    await userEvent.click(await (await openPicker()).findByRole('button', { name: 'Physical' }));
    await userEvent.click((await openPicker()).getByRole('button', { name: 'None' }));

    expect(onChange.mock.calls).toEqual([
      ['type', { guid: 'db1', localId: '20' }],
      ['type', null],
    ]);
  });

  it('narrows the list as you type', async () => {
    draw(fakeUnity(), damageTypeField('type', null));
    const list = await openPicker();
    await list.findByRole('button', { name: 'Physical' });

    await userEvent.type(screen.getByLabelText('Search type'), 'phy');

    expect(list.queryByRole('button', { name: 'Fire' })).not.toBeInTheDocument();
    expect(list.getByRole('button', { name: 'Physical' })).toBeInTheDocument();
  });

  it("opens the chosen item's own tab", async () => {
    const unity = fakeUnity();
    draw(unity, damageTypeField('type', PHYSICAL));

    await userEvent.click(await screen.findByRole('button', { name: 'Open Physical' }));

    expect(unity.openAsset).toHaveBeenCalledWith('GlobalObjectId_V1-2-db1-20-0');
  });

  it('asks Unity once for every field picking from the same database', async () => {
    const unity = fakeUnity();
    draw(unity, damageTypeField('first', PHYSICAL), damageTypeField('second', PHYSICAL));

    await screen.findAllByRole('button', { name: 'Open Physical' });

    expect(unity.searchAssets).toHaveBeenCalledTimes(1);
  });

  it("can't be changed when the field is read-only", () => {
    draw(
      fakeUnity(),
      damageTypeField('type', PHYSICAL, {
        hints: { ...damageTypeField('', null).hints, readOnly: true },
      }),
    );

    expect(screen.getByRole('button', { name: 'Pick type' })).toBeDisabled();
  });

  it('says why when Unity fails, and asks again on Retry', async () => {
    const searchAssets = vi
      .fn()
      .mockRejectedValueOnce(new Error('Unity is compiling'))
      .mockRejectedValueOnce(new Error('Unity is compiling'))
      .mockResolvedValue(databases);
    draw(fakeUnity({ searchAssets }), damageTypeField('type', null));

    await userEvent.click(screen.getByRole('button', { name: 'Pick type' }));
    await userEvent.click(await screen.findByRole('button', { name: 'Retry' }));

    expect(await screen.findByRole('button', { name: 'Physical' })).toBeInTheDocument();
  });
});
