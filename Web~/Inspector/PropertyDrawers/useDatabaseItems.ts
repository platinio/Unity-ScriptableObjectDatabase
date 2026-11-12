import { useCallback, useEffect, useRef, useState } from 'react';

import type { DatabaseItem, ItemCache } from './databaseItems.ts';

/** A database type's items as one field last heard them. */
export type ItemList =
  | { state: 'loading' }
  | { state: 'loaded'; items: DatabaseItem[] }
  | { state: 'failed'; problem: string };

/**
 * One field's view of a database type's items, from the page's shared cache. `refresh` asks Unity again. Only
 * the latest of several requests is kept, so a slow answer never replaces a newer one.
 */
export function useDatabaseItems(
  cache: ItemCache,
  databaseType: string,
): { list: ItemList; refresh(): void } {
  const [list, setList] = useState<ItemList>({ state: 'loading' });
  const latest = useRef(0);

  const keep = useCallback((answer: Promise<DatabaseItem[]>) => {
    const request = ++latest.current;
    answer.then(
      (items) => request === latest.current && setList({ state: 'loaded', items }),
      (error: unknown) =>
        request === latest.current &&
        setList({
          state: 'failed',
          problem: error instanceof Error ? error.message : String(error),
        }),
    );
  }, []);

  useEffect(() => {
    keep(cache.itemsOf(databaseType));
    // A field drawn again for another type, or taken off the page, ignores what was on its way.
    return () => {
      latest.current++;
    };
  }, [cache, databaseType, keep]);

  const refresh = useCallback(() => keep(cache.refresh(databaseType)), [cache, databaseType, keep]);
  return { list, refresh };
}
