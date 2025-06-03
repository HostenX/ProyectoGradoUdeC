mergeInto(LibraryManager.library, {
  GetLocalStorageItem: function(key) {
    var keyString = UTF8ToString(key);
    var item = localStorage.getItem(keyString);
    if (item === null) return null;
    var bufferSize = lengthBytesUTF8(item) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(item, buffer, bufferSize);
    return buffer;
  },
  
  // Opcionalmente, puedes añadir más funciones para manipular localStorage
  SetLocalStorageItem: function(key, value) {
    localStorage.setItem(UTF8ToString(key), UTF8ToString(value));
  },
  
  RemoveLocalStorageItem: function(key) {
    localStorage.removeItem(UTF8ToString(key));
  }
});