import { createRendererRegistry, PropertyForm, type PropertyNode } from '@hefesto/form';
import {
  rendererRegistrationsOf,
  UnityQuestionsProvider,
  type UnityQuestions,
} from '@hefesto/inspector';
import type { AssetInfo, UnityAsset } from '@hefesto/unity';
import { builtInWidgets } from '@hefesto/widgets';
import { act, render, renderHook, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import plugin from '../plugin.ts';
import { itemsOfDatabases, type DatabaseItem, type ItemCache } from './databaseItems.ts';
import { useDatabaseItems } from './useDatabaseItems.ts';

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
    id: `id-${localId}`,
    fields,
  };
}

const PHYSICAL_ITEM = itemOf('20', 'physical_item', {
  Name: 'Physical',
  icon: { guid: 'icons', localId: '5', name: 'sword', typeName: 'Sprite' },
});
const FIRE_ITEM = itemOf('10', 'Fire', { Name: '' });

function fileOf(guid: string, ...subAssets: ReturnType<typeof itemOf>[]): AssetInfo {
  return { guid, path: `Assets/${guid}.asset`, type: 'DamageTypeDatabase', subAssets };
}

const infos: Record<string, AssetInfo> = {
  db1: fileOf('db1', PHYSICAL_ITEM, FIRE_ITEM),
  db2: {
    ...fileOf('db2', itemOf('30', 'Holy', { Name: 'Holy' })),
    type: 'SpecialDamageTypeDatabase',
  },
};

/** A Unity that answers from `infos`, and whose connection a test can drop and bring back. */
function fakeUnity(overrides: Partial<UnityQuestions> = {}, isConnected = false) {
  let status = { connected: isConnected, playing: false, compiling: false };
  const listeners = new Set<() => void>();
  const unity = {
    searchAssets: vi.fn(async () => databases),
    assetInfo: vi.fn(async (guid: string) => infos[guid] ?? fileOf(guid)),
    openAsset: vi.fn(),
    bridge: {
      subscribeStatus: (listener: () => void) => {
        listeners.add(listener);
        return () => listeners.delete(listener);
      },
      getStatus: () => status,
      request: vi.fn(() => new Promise(() => undefined)),
    },
    connect(connected: boolean) {
      status = { ...status, connected };
      act(() => listeners.forEach((listener) => listener()));
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

async function openPicker(name: RegExp = /^type:/) {
  await userEvent.click(screen.getByRole('button', { name }));
  return within(await screen.findByRole('listbox', { name: `Items of ${DATABASE_TYPE}` }));
}

describe('ScriptableItemDatabaseSelector', () => {
  it('names the picker by its field and the chosen item, or None', () => {
    draw(fakeUnity(), damageTypeField('type', PHYSICAL), damageTypeField('other', null));

    expect(screen.getByRole('button', { name: 'type: physical_item' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'other: None' })).toBeInTheDocument();
  });

  it('lists None, then the items of databases of exactly that type, by their Name field', async () => {
    draw(fakeUnity(), damageTypeField('type', null));

    const list = await openPicker();

    await waitFor(() =>
      expect(
        list.getAllByRole('option').map((option) => option.getAttribute('aria-label')),
      ).toEqual(['None', 'Fire', 'Physical']),
    );
    expect(list.getByRole('option', { name: 'None' })).toHaveAttribute('aria-selected', 'true');
  });

  it('sends the picked item as its file and its place in the file, and None as null', async () => {
    const onChange = draw(fakeUnity(), damageTypeField('type', null));

    await userEvent.click(await (await openPicker()).findByRole('option', { name: 'Physical' }));
    await userEvent.click((await openPicker()).getByRole('option', { name: 'None' }));

    expect(onChange.mock.calls).toEqual([
      ['type', { guid: 'db1', localId: '20' }],
      ['type', null],
    ]);
  });

  it('narrows the list as you type, and Enter picks the only match', async () => {
    const onChange = draw(fakeUnity(), damageTypeField('type', null));
    const list = await openPicker();
    await list.findByRole('option', { name: 'Physical' });

    await userEvent.type(screen.getByLabelText('Search type'), 'phy');
    expect(list.queryByRole('option', { name: 'Fire' })).not.toBeInTheDocument();
    await userEvent.keyboard('{Enter}');

    expect(onChange).toHaveBeenCalledWith('type', { guid: 'db1', localId: '20' });
  });

  it("opens the chosen item's own tab, and offers no tab for None", async () => {
    const unity = fakeUnity();
    draw(unity, damageTypeField('type', PHYSICAL), damageTypeField('other', null));

    await userEvent.click(await screen.findByRole('button', { name: 'Open Physical' }));

    expect(unity.openAsset).toHaveBeenCalledWith('id-20');
    expect(screen.getAllByRole('button', { name: /^Open / })).toHaveLength(1);
  });

  it('asks Unity once for every field picking from the same database', async () => {
    const unity = fakeUnity();
    draw(unity, damageTypeField('first', PHYSICAL), damageTypeField('second', PHYSICAL));

    await screen.findAllByRole('button', { name: 'Open Physical' });

    expect(unity.searchAssets).toHaveBeenCalledTimes(1);
  });

  it('asks again when a list opens, so an item added in Unity shows up', async () => {
    const assetInfo = vi
      .fn()
      .mockResolvedValueOnce(fileOf('db1', PHYSICAL_ITEM))
      .mockResolvedValue(fileOf('db1', PHYSICAL_ITEM, FIRE_ITEM));
    draw(fakeUnity({ assetInfo }), damageTypeField('type', null));
    await waitFor(() => expect(assetInfo).toHaveBeenCalled());
    await screen.findByRole('button', { name: 'type: None' });

    const list = await openPicker();

    expect(await list.findByRole('option', { name: 'Fire' })).toBeInTheDocument();
  });

  it("asks again for a chosen item the list read earlier doesn't have yet", async () => {
    const assetInfo = vi
      .fn()
      .mockResolvedValueOnce(fileOf('db1', FIRE_ITEM))
      .mockResolvedValue(infos['db1']);
    draw(fakeUnity({ assetInfo }, true), damageTypeField('type', PHYSICAL));

    expect(await screen.findByRole('button', { name: 'Open Physical' })).toBeInTheDocument();
  });

  it('asks again when Unity reconnects after a failed list', async () => {
    const searchAssets = vi
      .fn()
      .mockRejectedValueOnce(new Error('Unity is compiling'))
      .mockResolvedValue(databases);
    const unity = fakeUnity({ searchAssets });
    draw(unity, damageTypeField('type', PHYSICAL));
    await waitFor(() => expect(searchAssets).toHaveBeenCalledTimes(1));

    unity.connect(true);

    expect(await screen.findByRole('button', { name: 'Open Physical' })).toBeInTheDocument();
  });

  it('looks for a still-missing choice again after Unity reconnects', async () => {
    const assetInfo = vi
      .fn()
      .mockResolvedValueOnce(fileOf('db1', FIRE_ITEM))
      .mockResolvedValueOnce(fileOf('db1', FIRE_ITEM))
      .mockResolvedValue(infos['db1']);
    const unity = fakeUnity({ assetInfo }, true);
    draw(unity, damageTypeField('type', PHYSICAL));
    await waitFor(() => expect(assetInfo).toHaveBeenCalledTimes(2));

    unity.connect(false);
    unity.connect(true);

    expect(await screen.findByRole('button', { name: 'Open Physical' })).toBeInTheDocument();
  });

  it('shares one new question among fields that all miss their choice', async () => {
    const assetInfo = vi.fn(async () => fileOf('db1', FIRE_ITEM));
    draw(
      fakeUnity({ assetInfo }, true),
      damageTypeField('first', PHYSICAL),
      damageTypeField('second', PHYSICAL),
    );

    await waitFor(() => expect(assetInfo).toHaveBeenCalledTimes(2));
    await new Promise((resolve) => setTimeout(resolve, 50));

    expect(assetInfo).toHaveBeenCalledTimes(2);
  });

  it("can't be changed when the field is read-only", () => {
    draw(
      fakeUnity(),
      damageTypeField('type', PHYSICAL, {
        hints: { ...damageTypeField('', null).hints, readOnly: true },
      }),
    );

    expect(screen.getByRole('button', { name: 'type: physical_item' })).toBeDisabled();
  });

  it('says why when Unity fails, and asks again on Retry', async () => {
    const searchAssets = vi
      .fn()
      .mockRejectedValueOnce(new Error('Unity is compiling'))
      .mockRejectedValueOnce(new Error('Unity is compiling'))
      .mockResolvedValue(databases);
    draw(fakeUnity({ searchAssets }), damageTypeField('type', null));

    await userEvent.click(screen.getByRole('button', { name: 'type: None' }));
    await userEvent.click(await screen.findByRole('button', { name: 'Retry' }));

    expect(await screen.findByRole('option', { name: 'Physical' })).toBeInTheDocument();
  });
});

describe("a database's items", () => {
  it('carry their icon sprite, and none when they have none', async () => {
    const items = await itemsOfDatabases(fakeUnity(), DATABASE_TYPE);

    expect(items.find(({ name }) => name === 'Physical')?.icon).toEqual({
      guid: 'icons',
      spriteName: 'sword',
    });
    expect(items.find(({ name }) => name === 'Fire')?.icon).toBeNull();
  });

  it('are none, with no search, for a field whose attribute names no database', async () => {
    const unity = fakeUnity();

    expect(await itemsOfDatabases(unity, '')).toEqual([]);
    expect(unity.searchAssets).not.toHaveBeenCalled();
  });

  it("keep one field's latest answer when an older one arrives after it", async () => {
    let answerTheFirst: (items: DatabaseItem[]) => void = () => undefined;
    const slow = new Promise<DatabaseItem[]>((resolve) => (answerTheFirst = resolve));
    const newer: DatabaseItem[] = [
      { id: 'new', guid: 'db1', localId: '1', name: 'Newer', icon: null },
    ];
    const cache = { itemsOf: () => slow, refresh: () => Promise.resolve(newer) } as ItemCache;
    const { result } = renderHook(() => useDatabaseItems(cache, DATABASE_TYPE));

    act(() => result.current.refresh());
    await waitFor(() => expect(result.current.list).toEqual({ state: 'loaded', items: newer }));
    await act(async () => answerTheFirst([]));

    expect(result.current.list).toEqual({ state: 'loaded', items: newer });
  });
});
