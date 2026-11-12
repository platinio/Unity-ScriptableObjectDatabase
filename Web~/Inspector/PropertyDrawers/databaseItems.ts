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
  return {
    itemsOf(databaseType: string): Promise<DatabaseItem[]> {
      let answer = answers.get(databaseType);
      if (answer === undefined) {
        answer = itemsOfDatabases(unity, databaseType);
        answers.set(databaseType, answer);
        answer.catch(() => answers.delete(databaseType));
      }
      return answer;
    },
    refresh(databaseType: string): Promise<DatabaseItem[]> {
      answers.delete(databaseType);
      return this.itemsOf(databaseType);
    },
  };
}

export type ItemCache = ReturnType<typeof createItemCache>;
