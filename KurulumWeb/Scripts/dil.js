function setFlag(langType) {
    switch (langType) {
        case "it-IT":
        case "it":
            $("#langEng").css("filter", "grayscale(80%)");
            $("#langEng").css("cursor", "pointer");
            $("#langIta").css("cursor", "default");
            $("#langIta").css("filter", "none");
            $("#langFra").css("filter", "grayscale(80%)");
            $("#langFra").css("cursor", "pointer");
            $("#langDe").css("filter", "grayscale(80%)");
            $("#langDe").css("cursor", "pointer");
            $("#langTr").css("filter", "grayscale(80%)");
            $("#langTr").css("cursor", "pointer");
            break;
        case "en-us":
        case "en":
            $("#langIta").css("filter", "grayscale(80%)");
            $("#langIta").css("cursor", "pointer");
            $("#langEng").css("cursor", "default");
            $("#langEng").css("filter", "none");
            $("#langFra").css("filter", "grayscale(80%)");
            $("#langFra").css("cursor", "pointer");
            $("#langDe").css("filter", "grayscale(80%)");
            $("#langDe").css("cursor", "pointer");
            $("#langTr").css("filter", "grayscale(80%)");
            $("#langTr").css("cursor", "pointer");
            break;
        case "fr-FR":
        case "fr":
            $("#langIta").css("filter", "grayscale(80%)");
            $("#langIta").css("cursor", "pointer");
            $("#langEng").css("filter", "grayscale(80%)");
            $("#langEng").css("cursor", "default");
            $("#langDe").css("filter", "grayscale(80%)");
            $("#langDe").css("cursor", "pointer");
            $("#langTr").css("filter", "grayscale(80%)");
            $("#langTr").css("cursor", "pointer");
            break;
        case "de-DE":
        case "de":
            $("#langIta").css("filter", "grayscale(80%)");
            $("#langIta").css("cursor", "pointer");
            $("#langEng").css("filter", "grayscale(80%)");
            $("#langEng").css("cursor", "default");
            $("#langFra").css("filter", "grayscale(80%)");
            $("#langFra").css("cursor", "pointer");
            $("#langTr").css("filter", "grayscale(80%)");
            $("#langTr").css("cursor", "pointer");
            break;
        case "tr-TR":
        case "tr":
            $("#langIta").css("filter", "grayscale(80%)");
            $("#langIta").css("cursor", "pointer");
            $("#langEng").css("filter", "grayscale(80%)");
            $("#langEng").css("cursor", "default");
            $("#langFra").css("filter", "grayscale(80%)");
            $("#langFra").css("cursor", "pointer");
            $("#langDe").css("filter", "grayscale(80%)");
            $("#langDe").css("cursor", "pointer");
            break;
    }
}