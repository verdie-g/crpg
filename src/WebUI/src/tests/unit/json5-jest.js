// eslint-disable-next-line @typescript-eslint/no-var-requires
const json5 = require('json5');

module.exports = {
  process: source => {
    let value;

    try {
      value = json5.parse(source);
    } catch (error) {
      console.error(error);
    }

    return value ? JSON.stringify(value) : source;
  },
};
