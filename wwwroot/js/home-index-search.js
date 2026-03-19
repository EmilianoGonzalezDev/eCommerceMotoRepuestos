(() => {
    const searchInput = document.getElementById("catalog-search-input");
    const searchForm = document.getElementById("catalog-search-form");
    const suggestionsList = document.getElementById("search-suggestions-list");
    const clearButton = document.getElementById("catalog-search-clear");
    const minChars = 2;
    const debounceDelayMs = 250;
    let debounceTimer;
    let activeSuggestionIndex = -1;

    if (!searchInput || !searchForm || !suggestionsList || !clearButton) return;

    function hideSuggestions() {
        suggestionsList.classList.add("d-none");
        suggestionsList.innerHTML = "";
        activeSuggestionIndex = -1;
    }

    function updateClearButtonVisibility() {
        const hasText = searchInput.value.trim().length > 0;
        clearButton.classList.toggle("d-none", !hasText);
    }

    function getSuggestionButtons() {
        return Array.from(suggestionsList.querySelectorAll("button.list-group-item-action"));
    }

    function setActiveSuggestion(index) {
        const buttons = getSuggestionButtons();
        buttons.forEach((button, idx) => {
            button.classList.toggle("active", idx === index);
        });
        activeSuggestionIndex = index;
    }

    function submitSuggestion(itemName) {
        searchInput.value = itemName;
        updateClearButtonVisibility();
        hideSuggestions();
        searchForm.submit();
    }

    function createSuggestionItem(item) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "list-group-item list-group-item-action d-flex justify-content-between align-items-center";
        button.innerHTML = `<span>${item.name}</span><strong>${item.priceFormatted}</strong>`;
        button.addEventListener("click", () => submitSuggestion(item.name));

        const li = document.createElement("li");
        li.appendChild(button);
        return li;
    }

    function renderSuggestions(items) {
        if (!items || items.length === 0) {
            hideSuggestions();
            return;
        }

        suggestionsList.innerHTML = "";
        items.forEach(item => {
            suggestionsList.appendChild(createSuggestionItem(item));
        });

        suggestionsList.classList.remove("d-none");
    }

    async function fetchSuggestions(value) {
        const response = await fetch(`/Home/SearchSuggestions?value=${encodeURIComponent(value)}`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });

        if (!response.ok) {
            return [];
        }

        return response.json();
    }

    function handleSearchInput() {
        const value = searchInput.value.trim();
        updateClearButtonVisibility();
        clearTimeout(debounceTimer);

        if (value.length < minChars) {
            hideSuggestions();
            return;
        }

        debounceTimer = setTimeout(async () => {
            try {
                const items = await fetchSuggestions(value);
                renderSuggestions(items);
            } catch {
                hideSuggestions();
            }
        }, debounceDelayMs);
    }

    function handleSearchKeyDown(event) {
        const buttons = getSuggestionButtons();
        const hasSuggestions = !suggestionsList.classList.contains("d-none") && buttons.length > 0;

        if (event.key === "ArrowDown" && hasSuggestions) {
            event.preventDefault();
            const nextIndex = activeSuggestionIndex < buttons.length - 1 ? activeSuggestionIndex + 1 : 0;
            setActiveSuggestion(nextIndex);
            return;
        }

        if (event.key === "ArrowUp" && hasSuggestions) {
            event.preventDefault();
            const nextIndex = activeSuggestionIndex > 0 ? activeSuggestionIndex - 1 : buttons.length - 1;
            setActiveSuggestion(nextIndex);
            return;
        }

        if (event.key === "Enter" && hasSuggestions && activeSuggestionIndex >= 0) {
            event.preventDefault();
            buttons[activeSuggestionIndex].click();
            return;
        }

        if (event.key === "Escape") {
            hideSuggestions();
        }
    }

    function handleClearClick() {
        searchInput.value = "";
        updateClearButtonVisibility();
        hideSuggestions();
        searchInput.focus();
    }

    function handleDocumentClick(event) {
        if (event.target !== searchInput && !suggestionsList.contains(event.target)) {
            hideSuggestions();
        }
    }

    searchInput.addEventListener("input", handleSearchInput);
    searchInput.addEventListener("keydown", handleSearchKeyDown);
    clearButton.addEventListener("click", handleClearClick);
    document.addEventListener("click", handleDocumentClick);

    updateClearButtonVisibility();
})();
