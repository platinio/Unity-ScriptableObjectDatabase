import type { RendererProps } from '@hefesto/form';
import { referenceOf, useUnity, type UnityQuestions } from '@hefesto/inspector';
import { Button, Popover } from '@hefesto/primitives';
import { AssetThumbnail, createThumbnailCache, type ThumbnailCache } from '@hefesto/unity';
import { useEffect, useState, useSyncExternalStore } from 'react';

import styles from './DatabaseItemDrawer.module.css';
import {
  createItemCache,
  type DatabaseItem,
  type ItemCache,
  type ItemIcon,
} from './databaseItems.ts';

const ATTRIBUTE = 'ScriptableItemDatabaseSelector';
const NONE = 'None';

type Loading =
  | { state: 'loading' }
  | { state: 'loaded'; items: DatabaseItem[] }
  | { state: 'failed'; problem: string };

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

/**
 * Draws a `[ScriptableItemDatabaseSelector]` reference as a searchable list of one database type's items,
 * with their icons and None, and a button that opens the chosen item's own tab.
 */
export function DatabaseItemDrawer({ node, readOnly, onChange }: RendererProps) {
  const unity = useUnity();
  const { items: cache, thumbnails } = cachesFor(unity);
  const databaseType = String(node.hints?.attributeValues?.[ATTRIBUTE]?.['DatabaseType'] ?? '');
  const chosen = referenceOf(node);
  const [loading, setLoading] = useState<Loading>({ state: 'loading' });
  const [isOpen, setIsOpen] = useState(false);
  const [search, setSearch] = useState('');

  function load(answer: Promise<DatabaseItem[]>, isCurrent: () => boolean = () => true) {
    answer.then(
      (items) => isCurrent() && setLoading({ state: 'loaded', items }),
      (error: unknown) =>
        isCurrent() &&
        setLoading({
          state: 'failed',
          problem: error instanceof Error ? error.message : String(error),
        }),
    );
  }

  useEffect(() => {
    let isCurrent = true;
    load(cache.itemsOf(databaseType), () => isCurrent);
    return () => {
      isCurrent = false;
    };
    // load only reads its arguments and setLoading, which never changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [cache, databaseType]);

  const items = loading.state === 'loaded' ? loading.items : [];
  const chosenItem = items.find(
    (item) => chosen !== null && item.guid === chosen.guid && item.localId === chosen.localId,
  );

  function open(isOpening: boolean) {
    setIsOpen(isOpening);
    setSearch('');
    if (isOpening) load(cache.refresh(databaseType));
  }

  function pick(item: DatabaseItem | null) {
    onChange(item === null ? null : { guid: item.guid, localId: item.localId });
    setIsOpen(false);
  }

  const shown = items.filter((item) =>
    item.name.toLowerCase().includes(search.trim().toLowerCase()),
  );

  return (
    <span className={styles['picker']}>
      <ItemIconView icon={chosenItem?.icon ?? null} thumbnails={thumbnails} unity={unity} />
      <Popover
        open={isOpen}
        onOpenChange={open}
        trigger={
          <Button
            className={styles['choice']}
            disabled={readOnly}
            aria-label={`Pick ${node.label}`}
          >
            {chosen?.name ?? NONE}
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
          />
          {loading.state === 'failed' ? (
            <p role="alert" className={styles['problem']}>
              Unity didn't list the items: {loading.problem}{' '}
              <Button variant="quiet" onClick={() => load(cache.refresh(databaseType))}>
                Retry
              </Button>
            </p>
          ) : (
            <ul aria-label={`Items of ${databaseType}`}>
              <li>
                <Button variant="quiet" aria-pressed={chosen === null} onClick={() => pick(null)}>
                  {NONE}
                </Button>
              </li>
              {shown.map((item) => (
                <li key={item.id}>
                  <Button
                    variant="quiet"
                    aria-pressed={item === chosenItem}
                    onClick={() => pick(item)}
                  >
                    <ItemIconView icon={item.icon} thumbnails={thumbnails} unity={unity} />
                    {item.name}
                  </Button>
                </li>
              ))}
              {loading.state === 'loading' && <li className={styles['muted']}>Asking Unity…</li>}
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

function ItemIconView({
  icon,
  thumbnails,
  unity,
}: {
  icon: ItemIcon | null;
  thumbnails: ThumbnailCache;
  unity: UnityQuestions;
}) {
  const isConnected = useSyncExternalStore(
    unity.bridge.subscribeStatus,
    unity.bridge.getStatus,
  ).connected;
  if (icon === null) return <span className={styles['noIcon']} aria-hidden="true" />;
  return (
    <AssetThumbnail
      asset={{ guid: icon.guid, type: 'Sprite', subAsset: icon.spriteName }}
      thumbnails={thumbnails}
      isAvailable={isConnected}
    />
  );
}
