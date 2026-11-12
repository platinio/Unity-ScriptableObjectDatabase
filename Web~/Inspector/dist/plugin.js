var hefestoModulePlugin = (function(_hefesto_inspector, _hefesto_primitives, _hefesto_unity, react, react_jsx_runtime) {
	var DatabaseItemDrawer_module_default = {
		picker: "arcaneonyx-scriptableobjectdatabase-PropertyDrawers-DatabaseItemDrawer-picker",
		choice: "arcaneonyx-scriptableobjectdatabase-PropertyDrawers-DatabaseItemDrawer-choice",
		list: "arcaneonyx-scriptableobjectdatabase-PropertyDrawers-DatabaseItemDrawer-list",
		option: "arcaneonyx-scriptableobjectdatabase-PropertyDrawers-DatabaseItemDrawer-option",
		search: "arcaneonyx-scriptableobjectdatabase-PropertyDrawers-DatabaseItemDrawer-search",
		noIcon: "arcaneonyx-scriptableobjectdatabase-PropertyDrawers-DatabaseItemDrawer-noIcon",
		problem: "arcaneonyx-scriptableobjectdatabase-PropertyDrawers-DatabaseItemDrawer-problem",
		muted: "arcaneonyx-scriptableobjectdatabase-PropertyDrawers-DatabaseItemDrawer-muted"
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
		if (databaseType === "") return [];
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
		const onTheirWay = /* @__PURE__ */ new Set();
		function ask(databaseType) {
			const answer = itemsOfDatabases(unity, databaseType);
			answers.set(databaseType, answer);
			onTheirWay.add(answer);
			answer.then(() => onTheirWay.delete(answer), () => {
				onTheirWay.delete(answer);
				if (answers.get(databaseType) === answer) answers.delete(databaseType);
			});
			return answer;
		}
		return {
			itemsOf(databaseType) {
				return answers.get(databaseType) ?? ask(databaseType);
			},
			/** Asks Unity again, unless a question about this type is already on its way, whose answer is new enough. */
			refresh(databaseType) {
				const current = answers.get(databaseType);
				return current !== void 0 && onTheirWay.has(current) ? current : ask(databaseType);
			}
		};
	}
	// PropertyDrawers/useDatabaseItems.ts
	/**
	* One field's view of a database type's items, from the page's shared cache. `refresh` asks Unity again. Only
	* the latest of several requests is kept, so a slow answer never replaces a newer one.
	*/
	function useDatabaseItems(cache, databaseType) {
		const [list, setList] = (0, react.useState)({ state: "loading" });
		const latest = (0, react.useRef)(0);
		const keep = (0, react.useCallback)((answer) => {
			const request = ++latest.current;
			answer.then((items) => request === latest.current && setList({
				state: "loaded",
				items
			}), (error) => request === latest.current && setList({
				state: "failed",
				problem: error instanceof Error ? error.message : String(error)
			}));
		}, []);
		(0, react.useEffect)(() => {
			keep(cache.itemsOf(databaseType));
			return () => {
				latest.current++;
			};
		}, [
			cache,
			databaseType,
			keep
		]);
		return {
			list,
			refresh: (0, react.useCallback)(() => keep(cache.refresh(databaseType)), [
				cache,
				databaseType,
				keep
			])
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
	function isTheItem(item, reference) {
		return reference !== null && item.guid === reference.guid && item.localId === reference.localId;
	}
	/**
	* Draws a `[ScriptableItemDatabaseSelector]` reference as a searchable list of one database type's items,
	* with their icons and None, and a button that opens the chosen item's own tab.
	*/
	function DatabaseItemDrawer({ node, readOnly, onChange }) {
		const unity = (0, _hefesto_inspector.useUnity)();
		const { items: cache, thumbnails } = cachesFor(unity);
		const isConnected = (0, react.useSyncExternalStore)(unity.bridge.subscribeStatus, unity.bridge.getStatus).connected;
		const databaseType = String(node.hints?.attributeValues?.[ATTRIBUTE]?.["DatabaseType"] ?? "");
		const chosen = (0, _hefesto_inspector.referenceOf)(node);
		const { list, refresh } = useDatabaseItems(cache, databaseType);
		const [isOpen, setIsOpen] = (0, react.useState)(false);
		const [search, setSearch] = (0, react.useState)("");
		const items = list.state === "loaded" ? list.items : [];
		const chosenItem = items.find((item) => isTheItem(item, chosen));
		useLookAgainWhenTheChoiceIsMissing({
			isMissing: list.state === "failed" || list.state === "loaded" && chosen !== null && chosenItem === void 0,
			choice: chosen === null ? "" : `${chosen.guid}/${chosen.localId}`,
			isConnected,
			refresh
		});
		const shown = items.filter((item) => item.name.toLowerCase().includes(search.trim().toLowerCase()));
		const chosenName = chosen?.name ?? NONE;
		function changeOpen(isOpening) {
			setIsOpen(isOpening);
			setSearch("");
			if (isOpening) refresh();
		}
		function pick(item) {
			onChange(item === null ? null : {
				guid: item.guid,
				localId: item.localId
			});
			setIsOpen(false);
		}
		function pickTheOnlyMatch(event) {
			const onlyMatch = shown.length === 1 ? shown[0] : void 0;
			if (event.key === "Enter" && onlyMatch !== void 0) pick(onlyMatch);
		}
		return /* @__PURE__ */ (0, react_jsx_runtime.jsxs)("span", {
			className: DatabaseItemDrawer_module_default["picker"],
			children: [
				/* @__PURE__ */ (0, react_jsx_runtime.jsx)(ItemIconView, {
					icon: chosenItem?.icon ?? null,
					thumbnails,
					isConnected
				}),
				/* @__PURE__ */ (0, react_jsx_runtime.jsx)(_hefesto_primitives.Popover, {
					open: isOpen,
					onOpenChange: changeOpen,
					trigger: /* @__PURE__ */ (0, react_jsx_runtime.jsx)(_hefesto_primitives.Button, {
						className: DatabaseItemDrawer_module_default["choice"],
						disabled: readOnly,
						"aria-label": `${node.label}: ${chosenName}`,
						children: chosenName
					}),
					children: /* @__PURE__ */ (0, react_jsx_runtime.jsxs)("div", {
						className: DatabaseItemDrawer_module_default["list"],
						children: [/* @__PURE__ */ (0, react_jsx_runtime.jsx)("input", {
							className: DatabaseItemDrawer_module_default["search"],
							"aria-label": `Search ${node.label}`,
							placeholder: "Search",
							value: search,
							onChange: (event) => setSearch(event.target.value),
							onKeyDown: pickTheOnlyMatch
						}), list.state === "failed" ? /* @__PURE__ */ (0, react_jsx_runtime.jsxs)("p", {
							role: "alert",
							className: DatabaseItemDrawer_module_default["problem"],
							children: [
								"Unity didn't list the items: ",
								list.problem,
								" ",
								/* @__PURE__ */ (0, react_jsx_runtime.jsx)(_hefesto_primitives.Button, {
									variant: "quiet",
									onClick: refresh,
									children: "Retry"
								})
							]
						}) : /* @__PURE__ */ (0, react_jsx_runtime.jsxs)("ul", {
							role: "listbox",
							"aria-label": `Items of ${databaseType}`,
							children: [
								/* @__PURE__ */ (0, react_jsx_runtime.jsx)(Option, {
									isChosen: chosen === null,
									name: NONE,
									onPick: () => pick(null),
									children: NONE
								}),
								shown.map((item) => /* @__PURE__ */ (0, react_jsx_runtime.jsxs)(Option, {
									isChosen: item === chosenItem,
									name: item.name,
									onPick: () => pick(item),
									children: [/* @__PURE__ */ (0, react_jsx_runtime.jsx)(ItemIconView, {
										icon: item.icon,
										thumbnails,
										isConnected
									}), item.name]
								}, item.id)),
								list.state === "loading" && /* @__PURE__ */ (0, react_jsx_runtime.jsx)("li", {
									role: "presentation",
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
	/**
	* A chosen item missing from the list was most likely added in Unity after the list was read, and a failed
	* list was most likely asked for while Unity was compiling. Either is asked again once per choice, and
	* again each time Unity reconnects.
	*/
	function useLookAgainWhenTheChoiceIsMissing({ isMissing, choice, isConnected, refresh }) {
		const lookedAgainFor = (0, react.useRef)(null);
		(0, react.useEffect)(() => {
			if (!isConnected) {
				lookedAgainFor.current = null;
				return;
			}
			if (!isMissing || lookedAgainFor.current === choice) return;
			lookedAgainFor.current = choice;
			refresh();
		}, [
			isMissing,
			choice,
			isConnected,
			refresh
		]);
	}
	function Option({ isChosen, name, onPick, children }) {
		return /* @__PURE__ */ (0, react_jsx_runtime.jsx)("li", {
			role: "option",
			"aria-selected": isChosen,
			"aria-label": name,
			tabIndex: 0,
			className: DatabaseItemDrawer_module_default["option"],
			onClick: onPick,
			onKeyDown: (event) => {
				if (event.key === "Enter" || event.key === " ") {
					event.preventDefault();
					onPick();
				}
			},
			children
		});
	}
	function ItemIconView({ icon, thumbnails, isConnected }) {
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
