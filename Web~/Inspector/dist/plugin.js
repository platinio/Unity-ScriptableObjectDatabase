var hefestoModulePlugin = (function(_hefesto_inspector, _hefesto_primitives, _hefesto_unity, react, react_jsx_runtime) {
	var DatabaseItemDrawer_module_default = {
		picker: "_picker_s6fik_1",
		choice: "_choice_s6fik_8",
		list: "_list_s6fik_14",
		search: "_search_s6fik_42",
		noIcon: "_noIcon_s6fik_46",
		problem: "_problem_s6fik_52",
		muted: "_muted_s6fik_57"
	};
	// PropertyDrawers/databaseItems.ts
	var ITEM_FIELDS = ["Name", "icon"];
	/** The class name without its namespace, which Unity's asset search narrows by, as the IMGUI drawer does. */
	function shortNameOf(typeName) {
		return typeName.slice(typeName.lastIndexOf(".") + 1);
	}
	/**
	* Every item of every database of exactly this type in the project, sorted by name. A database's items are
	* the ScriptableObjects inside its file.
	*/
	async function itemsOfDatabases(unity, databaseType) {
		const shortName = shortNameOf(databaseType);
		const databases = (await unity.searchAssets({
			q: "",
			assetType: shortName
		})).filter((asset) => asset.type === shortName);
		return (await Promise.all(databases.map((database) => unity.assetInfo(database.guid, ITEM_FIELDS)))).flatMap((file) => file.subAssets.filter((subAsset) => typeof subAsset.id === "string" && subAsset.id !== "").map((subAsset) => ({
			id: subAsset.id,
			guid: file.guid,
			localId: subAsset.localId,
			name: typeof subAsset.fields?.["Name"] === "string" && subAsset.fields["Name"] !== "" ? subAsset.fields["Name"] : subAsset.name,
			icon: iconOf(subAsset.fields?.["icon"])
		}))).sort((first, second) => first.name.localeCompare(second.name));
	}
	function iconOf(value) {
		if (value === null || typeof value !== "object") return null;
		const { guid, name } = value;
		return typeof guid === "string" && typeof name === "string" ? {
			guid,
			spriteName: name
		} : null;
	}
	/**
	* One answer per database type for the whole page, since many fields pick from the same database. Opening
	* a picker asks again, so an item added in Unity shows up.
	*/
	function createItemCache(unity) {
		const answers = /* @__PURE__ */ new Map();
		return {
			itemsOf(databaseType) {
				let answer = answers.get(databaseType);
				if (answer === void 0) {
					answer = itemsOfDatabases(unity, databaseType);
					answers.set(databaseType, answer);
					answer.catch(() => answers.delete(databaseType));
				}
				return answer;
			},
			refresh(databaseType) {
				answers.delete(databaseType);
				return this.itemsOf(databaseType);
			}
		};
	}
	// PropertyDrawers/DatabaseItemDrawer.tsx
	var ATTRIBUTE = "ScriptableItemDatabaseSelector";
	var NONE = "None";
	var itemCaches = /* @__PURE__ */ new WeakMap();
	var thumbnailCaches = /* @__PURE__ */ new WeakMap();
	function cachesFor(unity) {
		let items = itemCaches.get(unity);
		if (items === void 0) itemCaches.set(unity, items = createItemCache(unity));
		let thumbnails = thumbnailCaches.get(unity);
		if (thumbnails === void 0) thumbnailCaches.set(unity, thumbnails = (0, _hefesto_unity.createThumbnailCache)(unity.bridge));
		return {
			items,
			thumbnails
		};
	}
	/**
	* Draws a `[ScriptableItemDatabaseSelector]` reference as a searchable list of one database type's items,
	* with their icons and None, and a button that opens the chosen item's own tab.
	*/
	function DatabaseItemDrawer({ node, readOnly, onChange }) {
		const unity = (0, _hefesto_inspector.useUnity)();
		const { items: cache, thumbnails } = cachesFor(unity);
		const databaseType = String(node.hints?.attributeValues?.[ATTRIBUTE]?.["DatabaseType"] ?? "");
		const chosen = (0, _hefesto_inspector.referenceOf)(node);
		const [loading, setLoading] = (0, react.useState)({ state: "loading" });
		const [isOpen, setIsOpen] = (0, react.useState)(false);
		const [search, setSearch] = (0, react.useState)("");
		function load(answer, isCurrent = () => true) {
			answer.then((items) => isCurrent() && setLoading({
				state: "loaded",
				items
			}), (error) => isCurrent() && setLoading({
				state: "failed",
				problem: error instanceof Error ? error.message : String(error)
			}));
		}
		(0, react.useEffect)(() => {
			let isCurrent = true;
			load(cache.itemsOf(databaseType), () => isCurrent);
			return () => {
				isCurrent = false;
			};
		}, [cache, databaseType]);
		const items = loading.state === "loaded" ? loading.items : [];
		const chosenItem = items.find((item) => chosen !== null && item.guid === chosen.guid && item.localId === chosen.localId);
		function open(isOpening) {
			setIsOpen(isOpening);
			setSearch("");
			if (isOpening) load(cache.refresh(databaseType));
		}
		function pick(item) {
			onChange(item === null ? null : {
				guid: item.guid,
				localId: item.localId
			});
			setIsOpen(false);
		}
		const shown = items.filter((item) => item.name.toLowerCase().includes(search.trim().toLowerCase()));
		return /* @__PURE__ */ (0, react_jsx_runtime.jsxs)("span", {
			className: DatabaseItemDrawer_module_default["picker"],
			children: [
				/* @__PURE__ */ (0, react_jsx_runtime.jsx)(ItemIconView, {
					icon: chosenItem?.icon ?? null,
					thumbnails,
					unity
				}),
				/* @__PURE__ */ (0, react_jsx_runtime.jsx)(_hefesto_primitives.Popover, {
					open: isOpen,
					onOpenChange: open,
					trigger: /* @__PURE__ */ (0, react_jsx_runtime.jsx)(_hefesto_primitives.Button, {
						className: DatabaseItemDrawer_module_default["choice"],
						disabled: readOnly,
						"aria-label": `Pick ${node.label}`,
						children: chosen?.name ?? NONE
					}),
					children: /* @__PURE__ */ (0, react_jsx_runtime.jsxs)("div", {
						className: DatabaseItemDrawer_module_default["list"],
						children: [/* @__PURE__ */ (0, react_jsx_runtime.jsx)("input", {
							className: DatabaseItemDrawer_module_default["search"],
							"aria-label": `Search ${node.label}`,
							placeholder: "Search",
							value: search,
							onChange: (event) => setSearch(event.target.value)
						}), loading.state === "failed" ? /* @__PURE__ */ (0, react_jsx_runtime.jsxs)("p", {
							role: "alert",
							className: DatabaseItemDrawer_module_default["problem"],
							children: [
								"Unity didn't list the items: ",
								loading.problem,
								" ",
								/* @__PURE__ */ (0, react_jsx_runtime.jsx)(_hefesto_primitives.Button, {
									variant: "quiet",
									onClick: () => load(cache.refresh(databaseType)),
									children: "Retry"
								})
							]
						}) : /* @__PURE__ */ (0, react_jsx_runtime.jsxs)("ul", {
							"aria-label": `Items of ${databaseType}`,
							children: [
								/* @__PURE__ */ (0, react_jsx_runtime.jsx)("li", { children: /* @__PURE__ */ (0, react_jsx_runtime.jsx)(_hefesto_primitives.Button, {
									variant: "quiet",
									"aria-pressed": chosen === null,
									onClick: () => pick(null),
									children: NONE
								}) }),
								shown.map((item) => /* @__PURE__ */ (0, react_jsx_runtime.jsx)("li", { children: /* @__PURE__ */ (0, react_jsx_runtime.jsxs)(_hefesto_primitives.Button, {
									variant: "quiet",
									"aria-pressed": item === chosenItem,
									onClick: () => pick(item),
									children: [/* @__PURE__ */ (0, react_jsx_runtime.jsx)(ItemIconView, {
										icon: item.icon,
										thumbnails,
										unity
									}), item.name]
								}) }, item.id)),
								loading.state === "loading" && /* @__PURE__ */ (0, react_jsx_runtime.jsx)("li", {
									className: DatabaseItemDrawer_module_default["muted"],
									children: "Asking Unity…"
								})
							]
						})]
					})
				}),
				chosenItem !== void 0 && /* @__PURE__ */ (0, react_jsx_runtime.jsx)(_hefesto_primitives.Button, {
					variant: "quiet",
					"aria-label": `Open ${chosenItem.name}`,
					onClick: () => unity.openAsset(chosenItem.id),
					children: "►"
				})
			]
		});
	}
	function ItemIconView({ icon, thumbnails, unity }) {
		const isConnected = (0, react.useSyncExternalStore)(unity.bridge.subscribeStatus, unity.bridge.getStatus).connected;
		if (icon === null) return /* @__PURE__ */ (0, react_jsx_runtime.jsx)("span", {
			className: DatabaseItemDrawer_module_default["noIcon"],
			"aria-hidden": "true"
		});
		return /* @__PURE__ */ (0, react_jsx_runtime.jsx)(_hefesto_unity.AssetThumbnail, {
			asset: {
				guid: icon.guid,
				type: "Sprite",
				subAsset: icon.spriteName
			},
			thumbnails,
			isAvailable: isConnected
		});
	}
	return (0, _hefesto_inspector.definePlugin)({
		id: "arcaneonyx.scriptableobjectdatabase",
		drawers: [{
			id: "database-item",
			attribute: "ScriptableItemDatabaseSelector",
			component: DatabaseItemDrawer
		}]
	});
})(hefesto.inspector, hefesto.primitives, hefesto.unity, hefesto.react, hefesto.jsxRuntime);
