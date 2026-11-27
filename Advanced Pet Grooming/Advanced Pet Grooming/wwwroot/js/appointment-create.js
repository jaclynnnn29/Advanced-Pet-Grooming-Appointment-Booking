// @ts-nocheck

// 1. DOG GROOMING SERVICES (Priced by Size: S, M, L)
const dogPackages = [
    {
        name: "Dog Basic Grooming",
        desc: "Nail cut, paws trim, ear cleaning, sanitary trim, shower & dry, anal gland express & basic brush.",
        prices: { S: 115, M: 160, L: 210 }
    },
    {
        name: "Dog Full Grooming - Full Shave",
        desc: "Includes Dog Basic Grooming + full body shave.",
        prices: { S: 190, M: 230, L: 280 }
    },
    {
        name: "Dog Full Grooming - Lion Cut",
        desc: "Includes Dog Basic Grooming + Lion Cut style clip.",
        prices: { S: 200, M: 240, L: 310 }
    },
    {
        name: "Dog Full Grooming - Summer Cut",
        desc: "Includes Dog Basic Grooming + Summer Cut style clip.",
        prices: { S: 200, M: 250, L: 330 }
    },
    {
        name: "Dog Full Grooming - Hand Trim",
        desc: "Includes Dog Basic Grooming + full scissor hand trim.",
        prices: { S: 210, M: 260, L: 340 }
    }
];

const dogExtras = [
    { name: "Functioning Shampoo", price: 20 },
    { name: "Detick", price: 45 },
    { name: "Deshedding", price: 50 },
    { name: "Detangle", price: 50 }
];

// 2. CAT GROOMING SERVICES (Priced by Coat: Short Coat vs Long Coat)
const catPackages = [
    {
        name: "Cat Basic Grooming",
        desc: "Gentle bath, blow dry, brushing, nail trim, ear cleaning & sanitary trim.",
        prices: { "Short Coat": 120, "Long Coat": 140 }
    },
    {
        name: "Cat Full Grooming - Lion Cut",
        desc: "Includes Cat Basic Grooming + classic Lion Cut body trim.",
        prices: { "Short Coat": 180, "Long Coat": 200 }
    },
    {
        name: "Cat Full Grooming - Full Shave",
        desc: "Includes Cat Basic Grooming + smooth full-body shave trim.",
        prices: { "Short Coat": 180, "Long Coat": 200 }
    }
];

const catExtras = [
    { name: "Oatmeal Shampoo", price: 20 },
    { name: "Degreasing Care", price: 30 },
    { name: "Deflea Treatment", price: 40 },
    { name: "De-Shedding & Detangle", price: 45 }
];

const petSelect = document.getElementById("petSelect");
const serviceSection = document.getElementById("serviceSelectionSection");
const catCoatBox = document.getElementById("catCoatSelectorBox");
const packageContainer = document.getElementById("packageCardContainer");
const extrasContainer = document.getElementById("extrasContainer");
const hiddenPackageInput = document.getElementById("selectedServicePackageInput");
const tierBadge = document.getElementById("detectedTierBadge");
const menuFootnote = document.getElementById("menuFootnote");
const totalPriceDisplay = document.getElementById("totalPriceDisplay");

const dateInput = document.getElementById("appointmentDateInput");
const slotContainer = document.getElementById("timeSlotContainer");
const hiddenSlotInput = document.getElementById("selectedTimeSlotInput");
const submitBtn = document.getElementById("btnSubmitBooking");

let selectedPackageName = "";
let selectedExtras = new Set();

function normalizeDogSize(rawSize) {
    if (!rawSize) return "M";
    const s = rawSize.trim().toUpperCase();
    if (s === "XS" || s === "S" || s === "SMALL") return "S";
    if (s === "L" || s === "XL" || s === "LARGE") return "L";
    return "M";
}

function isCatSpecies(rawSpecies) {
    return rawSpecies && rawSpecies.trim().toLowerCase() === "cat";
}

function getSelectedCatCoat() {
    const checked = document.querySelector('input[name="catCoatLength"]:checked');
    return checked ? checked.value : "Short Coat";
}

function getActiveMenu(isCat) {
    return isCat ? catPackages : dogPackages;
}

function updateSubmitState() {
    const hasPet = petSelect && petSelect.value !== "";
    const hasPkg = hiddenPackageInput.value !== "";
    const hasSlot = hiddenSlotInput.value !== "";
    submitBtn.disabled = !(hasPet && hasPkg && hasSlot);
}

function updateFormattedPackageValue(isCat, tierKey) {
    const activeMenu = getActiveMenu(isCat);
    const activeExtras = isCat ? catExtras : dogExtras;
    const pkgObj = activeMenu.find(p => p.name === selectedPackageName);

    if (!pkgObj) {
        hiddenPackageInput.value = "";
        totalPriceDisplay.textContent = "Total: RM 0";
        return;
    }

    let total = pkgObj.prices[tierKey];
    const extrasList = [];
    activeExtras.forEach(ext => {
        if (selectedExtras.has(ext.name)) {
            total += ext.price;
            extrasList.push(ext.name);
        }
    });

    const tierLabel = isCat ? tierKey : `Size ${tierKey}`;
    const extrasText = extrasList.length > 0 ? ` + ${extrasList.join(", ")}` : "";
    let finalSummary = `${pkgObj.name} (${tierLabel})${extrasText} - RM ${total}`;

    // Keep within VARCHAR(100) column limit
    if (finalSummary.length > 100) {
        finalSummary = `${pkgObj.name} (${tierLabel}) + ${extrasList.length} Extra(s) - RM ${total}`;
    }

    hiddenPackageInput.value = finalSummary;
    totalPriceDisplay.textContent = `Total: RM ${total}`;
}

function renderExtras(isCat, tierKey) {
    const activeExtras = isCat ? catExtras : dogExtras;

    selectedExtras.forEach(name => {
        if (!activeExtras.some(e => e.name === name)) {
            selectedExtras.delete(name);
        }
    });

    extrasContainer.innerHTML = "";
    activeExtras.forEach(ext => {
        const col = document.createElement("div");
        col.className = "col-md-4 col-sm-6";

        const id = `extra_${ext.name.replace(/[^a-zA-Z0-9]/g, "")}`;
        const checked = selectedExtras.has(ext.name) ? "checked" : "";

        col.innerHTML = `
            <div class="form-check">
                <input class="form-check-input extra-checkbox" type="checkbox" id="${id}" value="${ext.name}" ${checked}>
                <label class="form-check-label small" for="${id}">
                    ${ext.name} (+RM ${ext.price})
                </label>
            </div>
        `;

        col.querySelector("input").addEventListener("change", (e) => {
            if (e.target.checked) selectedExtras.add(ext.name);
            else selectedExtras.delete(ext.name);
            updateFormattedPackageValue(isCat, tierKey);
            updateSubmitState();
        });

        extrasContainer.appendChild(col);
    });
}

function renderPackages() {
    const selectedOption = petSelect.options[petSelect.selectedIndex];
    const hasPet = selectedOption && selectedOption.value !== "";

    // Hide all services until user chooses a Dog or Cat
    if (!hasPet) {
        serviceSection.classList.add("d-none");
        catCoatBox.classList.add("d-none");
        selectedPackageName = "";
        selectedExtras.clear();
        hiddenPackageInput.value = "";
        updateSubmitState();
        return;
    }

    serviceSection.classList.remove("d-none");
    const rawSpecies = selectedOption.getAttribute("data-species") || "Dog";
    const isCat = isCatSpecies(rawSpecies);

    if (isCat) {
        catCoatBox.classList.remove("d-none");
    } else {
        catCoatBox.classList.add("d-none");
    }

    const tierKey = isCat
        ? getSelectedCatCoat()
        : normalizeDogSize(selectedOption.getAttribute("data-size"));

    if (isCat) {
        tierBadge.textContent = `Filtered: Cat Services (${tierKey})`;
        menuFootnote.textContent = "*Showing Cat grooming services only (Short Coat vs. Long Coat).";
    } else {
        tierBadge.textContent = `Filtered: Dog Services (Size ${tierKey})`;
        menuFootnote.textContent = `*Showing Dog grooming services only, matched to Size ${tierKey}.`;
    }

    const activeMenu = getActiveMenu(isCat);

    if (!activeMenu.some(p => p.name === selectedPackageName)) {
        selectedPackageName = "";
    }

    packageContainer.innerHTML = "";

    activeMenu.forEach(pkg => {
        const price = pkg.prices[tierKey];
        const badgeText = isCat ? `RM ${price} (${tierKey})` : `RM ${price} (Size ${tierKey})`;
        const isSelected = selectedPackageName === pkg.name;

        const col = document.createElement("div");
        col.className = "col-md-6";

        const card = document.createElement("button");
        card.type = "button";
        card.className = isSelected
            ? "btn w-100 text-start package-card package-card-selected"
            : "btn w-100 text-start package-card";

        card.innerHTML = `
            <div class="d-flex justify-content-between align-items-center mb-1">
                <span class="fw-bold">${pkg.name}</span>
                <span class="badge package-price-badge">${badgeText}</span>
            </div>
            <small class="d-block package-desc">${pkg.desc}</small>
        `;

        card.addEventListener("click", () => {
            selectedPackageName = pkg.name;
            renderPackages();
            updateSubmitState();
        });

        col.appendChild(card);
        packageContainer.appendChild(col);
    });

    renderExtras(isCat, tierKey);
    updateFormattedPackageValue(isCat, tierKey);
    updateSubmitState();
}

async function loadTimeSlots(selectedDate) {
    if (!selectedDate) return;
    slotContainer.innerHTML = "<div class='text-muted'>Checking slot availability...</div>";
    hiddenSlotInput.value = "";
    updateSubmitState();

    const response = await fetch(`/Appointment/GetSlotAvailability?date=${selectedDate}`);
    const slots = await response.json();

    slotContainer.innerHTML = "";
    slots.forEach(slot => {
        const col = document.createElement("div");
        col.className = "col-md-4 col-sm-6";

        const btn = document.createElement("button");
        btn.type = "button";
        btn.className = slot.isFull
            ? "btn w-100 text-start slot-card slot-card-full"
            : "btn w-100 text-start slot-card slot-card-available";

        btn.disabled = slot.isFull;
        btn.innerHTML = `
            <div class="fw-bold">${slot.timeSlot}</div>
            <small>${slot.isFull ? "FULL (5/5) - Unavailable" : `${slot.remaining} of 5 slots left`}</small>
        `;

        if (!slot.isFull) {
            btn.addEventListener("click", () => {
                document.querySelectorAll(".slot-card-available").forEach(b => b.classList.remove("slot-card-selected"));
                btn.classList.add("slot-card-selected");
                hiddenSlotInput.value = slot.timeSlot;
                updateSubmitState();
            });
        }

        col.appendChild(btn);
        slotContainer.appendChild(col);
    });
}

petSelect.addEventListener("change", renderPackages);
document.querySelectorAll('input[name="catCoatLength"]').forEach(r => {
    r.addEventListener("change", renderPackages);
});
dateInput.addEventListener("change", () => loadTimeSlots(dateInput.value));

renderPackages();
loadTimeSlots(dateInput.value);

// Staff Walk-In Customer Search (DFD 3.1)
const btnSearch = document.getElementById("btnSearchCustomer");
if (btnSearch) {
    btnSearch.addEventListener("click", async () => {
        const q = document.getElementById("customerSearchInput").value;
        const res = await fetch(`/Appointment/SearchCustomerPets?query=${encodeURIComponent(q)}`);
        const customers = await res.json();
        const resultsDiv = document.getElementById("customerSearchResults");
        resultsDiv.innerHTML = "";

        customers.forEach(c => {
            const item = document.createElement("button");
            item.type = "button";
            item.className = "list-group-item list-group-item-action";
            item.textContent = `${c.name} (${c.phone}) — ${c.pets.length} pet(s)`;
            item.addEventListener("click", () => {
                document.getElementById("selectedCustomerId").value = c.customerId;
                const banner = document.getElementById("selectedCustomerBanner");
                banner.textContent = `Selected Customer: ${c.name} (${c.phone})`;
                banner.classList.remove("d-none");

                petSelect.innerHTML = "";
                if (c.pets.length === 0) {
                    const emptyOpt = document.createElement("option");
                    emptyOpt.value = "";
                    emptyOpt.textContent = "-- No pets registered for this customer --";
                    petSelect.appendChild(emptyOpt);
                } else {
                    c.pets.forEach(p => {
                        const opt = document.createElement("option");
                        const pSpecies = p.petType || "Dog";
                        const pSize = normalizeDogSize(p.sizeCategory);
                        const isCat = isCatSpecies(pSpecies);

                        opt.value = p.petId;
                        opt.setAttribute("data-species", pSpecies);
                        opt.setAttribute("data-size", pSize);
                        opt.textContent = isCat
                            ? `${p.petName} (Cat)`
                            : `${p.petName} (Dog - Size ${pSize})`;
                        petSelect.appendChild(opt);
                    });
                }

                resultsDiv.innerHTML = "";
                renderPackages();
            });
            resultsDiv.appendChild(item);
        });
    });
}