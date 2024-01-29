import { createTheme } from "@mui/material/styles";
import * as locales from "@mui/material/locale";

export default class GlobalHelper {

  static initMuiTheme(locale) {

    var themeDefinition = {
      typography: {
        body1: {
          fontFamily: "'Nunito', sans-serif"
        },
        button: {
          textTransform: "none"
        }
      },
      palette: {
        primary: {
          main: "#1B5A6E"
        },
        secondary: {
          main: "#1B5A6E"
        }
      }
    };

    if (locale) {

      const localeMappings = {
        "pl": "plPL",
        "de": "deDE",
        "en": "enUS"
      };

      return createTheme(themeDefinition, locales[localeMappings[locale]]);
    }
    else {
      
      return createTheme(themeDefinition);
    }
  }
}
