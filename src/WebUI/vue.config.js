const CompressionPlugin = require('compression-webpack-plugin'); // eslint-disable-line @typescript-eslint/no-var-requires

module.exports = {
  lintOnSave: 'warning',
  chainWebpack(config) {
    // https://medium.com/@aetherus.zhou/vue-cli-3-performance-optimization-55316dcd491c
    // disable prefetch of chunks (usually pages) to only load required chunks for the current page
    config.plugins.delete('prefetch');

    if (process.env.NODE_ENV === 'production') {
      // compress files on build
      config.plugin('CompressionPlugin').use(CompressionPlugin);
    }
  },
};
