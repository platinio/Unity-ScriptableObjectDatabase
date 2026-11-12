# ScriptableObjectDatabase inspector plugin

ScriptableObjectDatabase's drawers for Hefesto's web inspector: a `[ScriptableItemDatabaseSelector]`
field picks from its database's items, with their icons, as the IMGUI dropdown does. Unity finds this folder by its
`hefesto-plugin.json` and the inspector page loads `dist/plugin.js`, so the module brings its own
look without the project's inspector app changing. See
[Custom web editors](../../../Hefesto/Web~/../Documentation~/guides/custom-web-editors.md).

| Command                  | What it does                                                    |
| ------------------------ | --------------------------------------------------------------- |
| `npm test`               | Runs the drawers' tests                                         |
| `npm run build`          | Checks the types and builds `dist/plugin.js` and `plugin.css`   |
| `npm run watch`          | Builds on every save; the open inspector page reloads by itself |
| `npm run update-hefesto` | Rebuilds Hefesto's packages after updating it                   |

| File                  | What it holds                                                   |
| --------------------- | --------------------------------------------------------------- |
| `hefesto-plugin.json` | The plugin's id and the plugin API it is built for              |
| `plugin.ts`           | What the plugin adds, passed to `definePlugin`                  |
| `PropertyDrawers/`    | Drawers: each draws one field, wherever it appears              |
| `Editors/`            | Editors, when you add one: each draws a whole asset of one type |
| `dist/`               | The build. Commit it, so people who use the module need no Node |
