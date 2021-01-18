const CompressionPlugin = require('compression-webpack-plugin'); // eslint-disable-line @typescript-eslint/no-var-requires

function useJson5Loader(config) {
  const jsonRule = config.module.rule('json');

  // Clear all existing loaders. If you don't do this, the loader below will be
  // appended to existing loaders of the rule.
  jsonRule.uses.clear();

  // Add replacement loader.
  jsonRule
    .test(/\.json$/).type('javascript/auto')
    .use('json5-loader')
    .loader('json5-loader');
}

const pages = {
  index: {
    entry:'src/main.ts',
    chunks: ['chunk-index-vendors', 'chunk-common', 'index']
  },
  'silent-renew': {
    entry: 'src/silent-renew.ts',
    chunks:['chunk-silent-renew-vendors', 'chunk-common', 'silent-renew']
  },
};

module.exports = {
  lintOnSave: 'warning',
  pages,
  chainWebpack(config) {
    // https://medium.com/@aetherus.zhou/vue-cli-3-performance-optimization-55316dcd491c
    // disable prefetch of chunks (usually pages) to only load required chunks for the current page
    config.plugins.delete('prefetch');

    // Default JSON loader doesn't support comments.
    useJson5Loader(config);

    if (process.env.NODE_ENV === 'production') {
      // compress files on build
      config.plugin('CompressionPlugin').use(CompressionPlugin);
    }

    const pageKeys = Object.keys(pages);
    const IS_VENDOR = /[\\/]node_modules[\\/]/;

    const cacheGroups = {
      ...pageKeys.map(key => ({
        name: `chunk-${key}-vendors`,
        priority: -9,
        chunks: chunk => chunk.name === key,
        test: IS_VENDOR,
        enforce: true,
      })),
      common: {
        name: 'chunk-common',
        priority: -20,
        chunks: 'initial',
        minChunks: 2,
        reuseExistingChunk: true,
        enforce: true,
      },
    };

    config.optimization
        .splitChunks({
            cacheGroups
    });

  },
};
