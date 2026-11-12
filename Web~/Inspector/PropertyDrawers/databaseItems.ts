import type { UnityQuestions } from '@hefesto/inspector';

/** An item of a database, as the picker lists it. */
export interface DatabaseItem {
  /** What the web inspector opens it by. */
  id: string;
  guid: string;
  localId: string;
  /** The item's `Name` field, which the database window shows, or its object's name without one. */
  name: string;
  /** Its `icon` field, a sprite reference, when it has one. */
  icon: ItemIcon | null;
}

export interface ItemIcon {
  guid: string;
  /** The sprite's name inside its texture's file, which Unity's thumbnails are asked by. */
  spriteName: string;
}

const ITEM_FIELDS = ['Name', 'icon'];

/** The class name without its namespace, which Unity's asset search narrows by, as the IMGUI drawer does. */
function shortNameOf(typeName: string): string {
  return typeName.slice(typeName.lastIndexOf('.') + 1);
}

/**
 * Every item of every database of exactly this type in the project, sorted by name. A database's items are
 * the ScriptableObjects inside its file.
 */
export async function itemsOfDatabases(
  unity: UnityQuestions,
  databaseType: string,
): Promise<DatabaseItem[]> {
  if (databaseType === '') return [];
  const shortName = shortNameOf(databaseType);
  const databases = (await unity.searchAssets({ q: '', assetType: shortName })).filter(
    (asset) => asset.type === shortName,
  );
  const files = await Promise.all(
    databases.map((database) => unity.assetInfo(database.guid, ITEM_FIELDS)),
  );
  return files
    .flatMap((file) =>
      file.subAssets
        .filter((subAsset) => typeof subAsset.id === 'string' && subAsset.id !== '')
        .map((subAsset) => ({
          id: subAsset.id,
          guid: file.guid,
          localId: subAsset.localId,
          name:
            typeof subAsset.fields?.['Name'] === 'string' && subAsset.fields['Name'] !== ''
              ? subAsset.fields['Name']
              : subAsset.name,
          icon: iconOf(subAsset.fields?.['icon']),
        })),
    )
    .sort((first, second) => first.name.localeCompare(second.name));
}

function iconOf(value: unknown): ItemIcon | null {
  if (value === null || typeof value !== 'object') return null;
  const { guid, name } = value as { guid?: unknown; name?: unknown };
  return typeof guid === 'string' && typeof name === 'string' ? { guid, spriteName: name } : null;
}

/**
 * One answer per database type for the whole page, since many fields pick from the same database. Opening
 * a picker asks again, so an item added in Unity shows up.
 */
export function createItemCache(unity: UnityQuestions) {
  const answers = new Map<string, Promise<DatabaseItem[]>>();
  const onTheirWay = new Set<Promise<DatabaseItem[]>>();

  function ask(databaseType: string): Promise<DatabaseItem[]> {
    const answer = itemsOfDatabases(unity, databaseType);
    answers.set(databaseType, answer);
    onTheirWay.add(answer);
    answer.then(
      () => onTheirWay.delete(answer),
      () => {
        onTheirWay.delete(answer);
        // A newer answer may already be kept; only this one is forgotten.
        if (answers.get(databaseType) === answer) answers.delete(databaseType);
      },
    );
    return answer;
  }

  return {
    itemsOf(databaseType: string): Promise<DatabaseItem[]> {
      return answers.get(databaseType) ?? ask(databaseType);
    },
    /** Asks Unity again, unless a question about this type is already on its way, whose answer is new enough. */
    refresh(databaseType: string): Promise<DatabaseItem[]> {
      const current = answers.get(databaseType);
      return current !== undefined && onTheirWay.has(current) ? current : ask(databaseType);
    },
  };
}

export type ItemCache = ReturnType<typeof createItemCache>;
