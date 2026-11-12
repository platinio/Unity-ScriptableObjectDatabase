import { definePlugin } from '@hefesto/inspector';

import { DatabaseItemDrawer } from './PropertyDrawers/DatabaseItemDrawer.tsx';

export default definePlugin({
  id: 'arcaneonyx.scriptableobjectdatabase',
  drawers: [
    {
      id: 'database-item',
      attribute: 'ScriptableItemDatabaseSelector',
      component: DatabaseItemDrawer,
    },
  ],
});
