module.exports = {
  lintOnSave: 'warning',
  // disable prefetch of chunks (usually pages) to only load required chunks for the current page
  // https://medium.com/@mrodal/how-to-make-lazy-loading-actually-work-in-vue-cli-3-7f3f88cfb102
  chainWebpack: config => config.plugins.delete('prefetch'),
};
