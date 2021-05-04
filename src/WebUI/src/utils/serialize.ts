export function parameterizeArray(key: string, arr: string[]) {
    arr = arr.map(encodeURIComponent)
    return key+'[]=' + arr.join('&'+key+'[]=')
  }