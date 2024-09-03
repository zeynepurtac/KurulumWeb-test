function setFlag(langType) {
    switch (langType) {
        case "it-IT":
        case "it":
            $("#langEng").css("filter", "grayscale(80%)");
            $("#langIta").css("cursor", "default");
            $("#langIta").css("filter", "none");
            $("#langEng").css("cursor", "pointer");
            break;
        case "en-us":
        case "en":
            $("#langIta").css("filter", "grayscale(80%)");
            $("#langIta").css("filter", "grayscale(80%)");
            $("#langEng").css("cursor", "default");
            $("#langEng").css("filter", "none");
            $("#langIta").css("cursor", "pointer");
            break;
    }
}