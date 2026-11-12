import type { RendererProps } from '@hefesto/form';
import {
  referenceOf,
  useUnity,
  type UnityQuestions,
  type UnityReference,
} from '@hefesto/inspector';
import { Button, Popover } from '@hefesto/primitives';
import { AssetThumbnail, createThumbnailCache, type ThumbnailCache } from '@hefesto/unity';
import {
  useEffect,
  useRef,
  useState,
  useSyncExternalStore,
  type KeyboardEvent,
  type ReactNode,
} from 'react';

import styles from './DatabaseItemDrawer.module.css';
import {
  createItemCache,
  type DatabaseItem,
  type ItemCache,
  type ItemIcon,
} from './databaseItems.ts';
import { useDatabaseItems } from './useDatabaseItems.ts';

const ATTRIBUTE = 'ScriptableItemDatabaseSelector';
const NONE = 'None';

// One cache of items and of thumbnails per page, shared by every field that picks from a database.
const itemCaches = new WeakMap<UnityQuestions, ItemCache>();
const thumbnailCaches = new WeakMap<UnityQuestions, ThumbnailCache>();

function cachesFor(unity: UnityQuestions): { items: ItemCache; thumbnails: ThumbnailCache } {
  let items = itemCaches.get(unity);
  if (items === undefined) itemCaches.set(unity, (items = createItemCache(unity)));
  let thumbnails = thumbnailCaches.get(unity);
  if (thumbnails === undefined)
    thumbnailCaches.set(unity, (thumbnails = createThumbnailCache(unity.bridge)));
  return { items, thumbnails };
}

function isTheItem(item: DatabaseItem, reference: UnityReference | null): boolean {
  return reference !== null && item.guid === reference.guid && item.localId === reference.localId;
}

/**
 * Draws a `[ScriptableItemDatabaseSelector]` reference as a searchable list of one database type's items,
 * with their icons and None, and a button that opens the chosen item's own tab.
 */
export function DatabaseItemDrawer({ node, readOnly, onChange }: RendererProps) {
  const unity = useUnity();
  const { items: cache, thumbnails } = cachesFor(unity);
  const isConnected = useSyncExternalStore(
    unity.bridge.subscribeStatus,
    unity.bridge.getStatus,
  ).connected;
  const databaseType = String(node.hints?.attributeValues?.[ATTRIBUTE]?.['DatabaseType'] ?? '');
  const chosen = referenceOf(node);
  const { list, refresh } = useDatabaseItems(cache, databaseType);
  const [isOpen, setIsOpen] = useState(false);
  const [search, setSearch] = useState('');

  const items = list.state === 'loaded' ? list.items : [];
  const chosenItem = items.find((item) => isTheItem(item, chosen));
  useLookAgainWhenTheChoiceIsMissing({
    isMissing:
      list.state === 'failed' ||
      (list.state === 'loaded' && chosen !== null && chosenItem === undefined),
    choice: chosen === null ? '' : `${chosen.guid}/${chosen.localId}`,
    isConnected,
    refresh,
  });

  const shown = items.filter((item) =>
    item.name.toLowerCase().includes(search.trim().toLowerCase()),
  );
  const chosenName = chosen?.name ?? NONE;

  function changeOpen(isOpening: boolean) {
    setIsOpen(isOpening);
    setSearch('');
    if (isOpening) refresh();
  }

  function pick(item: DatabaseItem | null) {
    onChange(item === null ? null : { guid: item.guid, localId: item.localId });
    setIsOpen(false);
  }

  function pickTheOnlyMatch(event: KeyboardEvent<HTMLInputElement>) {
    const onlyMatch = shown.length === 1 ? shown[0] : undefined;
    if (event.key === 'Enter' && onlyMatch !== undefined) pick(onlyMatch);
  }

  return (
    <span className={styles['picker']}>
      <ItemIconView
        icon={chosenItem?.icon ?? null}
        thumbnails={thumbnails}
        isConnected={isConnected}
      />
      <Popover
        open={isOpen}
        onOpenChange={changeOpen}
        trigger={
          <Button
            className={styles['choice']}
            disabled={readOnly}
            aria-label={`${node.label}: ${chosenName}`}
          >
            {chosenName}
          </Button>
        }
      >
        <div className={styles['list']}>
          <input
            className={styles['search']}
            aria-label={`Search ${node.label}`}
            placeholder="Search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            onKeyDown={pickTheOnlyMatch}
          />
          {list.state === 'failed' ? (
            <p role="alert" className={styles['problem']}>
              Unity didn't list the items: {list.problem}{' '}
              <Button variant="quiet" onClick={refresh}>
                Retry
              </Button>
            </p>
          ) : (
            <ul role="listbox" aria-label={`Items of ${databaseType}`}>
              <Option isChosen={chosen === null} name={NONE} onPick={() => pick(null)}>
                {NONE}
              </Option>
              {shown.map((item) => (
                <Option
                  key={item.id}
                  isChosen={item === chosenItem}
                  name={item.name}
                  onPick={() => pick(item)}
                >
                  <ItemIconView
                    icon={item.icon}
                    thumbnails={thumbnails}
                    isConnected={isConnected}
                  />
                  {item.name}
                </Option>
              ))}
              {list.state === 'loading' && (
                <li role="presentation" className={styles['muted']}>
                  Asking Unity…
                </li>
              )}
            </ul>
          )}
        </div>
      </Popover>
      {chosenItem !== undefined && (
        <Button
          variant="quiet"
          aria-label={`Open ${chosenItem.name}`}
          onClick={() => unity.openAsset(chosenItem.id)}
        >
          ►
        </Button>
      )}
    </span>
  );
}

/**
 * A chosen item missing from the list was most likely added in Unity after the list was read, and a failed
 * list was most likely asked for while Unity was compiling. Either is asked again once per choice, and
 * again each time Unity reconnects.
 */
function useLookAgainWhenTheChoiceIsMissing({
  isMissing,
  choice,
  isConnected,
  refresh,
}: {
  isMissing: boolean;
  choice: string;
  isConnected: boolean;
  refresh(): void;
}) {
  const lookedAgainFor = useRef<string | null>(null);
  useEffect(() => {
    if (!isConnected) {
      lookedAgainFor.current = null;
      return;
    }
    if (!isMissing || lookedAgainFor.current === choice) return;
    lookedAgainFor.current = choice;
    refresh();
  }, [isMissing, choice, isConnected, refresh]);
}

function Option({
  isChosen,
  name,
  onPick,
  children,
}: {
  isChosen: boolean;
  name: string;
  onPick(): void;
  children: ReactNode;
}) {
  return (
    <li
      role="option"
      aria-selected={isChosen}
      aria-label={name}
      tabIndex={0}
      className={styles['option']}
      onClick={onPick}
      onKeyDown={(event) => {
        if (event.key === 'Enter' || event.key === ' ') {
          event.preventDefault();
          onPick();
        }
      }}
    >
      {children}
    </li>
  );
}

function ItemIconView({
  icon,
  thumbnails,
  isConnected,
}: {
  icon: ItemIcon | null;
  thumbnails: ThumbnailCache;
  isConnected: boolean;
}) {
  if (icon === null) return <span className={styles['noIcon']} aria-hidden="true" />;
  return (
    <AssetThumbnail
      asset={{ guid: icon.guid, type: 'Sprite', subAsset: icon.spriteName }}
      thumbnails={thumbnails}
      isAvailable={isConnected}
    />
  );
}
