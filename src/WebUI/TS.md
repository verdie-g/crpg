# TS Tips

## fn params

```ts
function calculateGrade(course1: number, course2: number) : string {
    return (course1 + course2) > 70 ? 'A' : 'B';
}
```

## Interface

```ts
interface User {
  name: string;
  id: number;
}

const user: User = {}
```

## Union types
```ts
function getLength(obj: string | string[]) {
  return obj.length;
}
```

## Type Predicate

```ts
string	    typeof s === "string"
number	    typeof n === "number"
boolean	    typeof b === "boolean"
undefined	typeof undefined === "undefined"
function    typeof f === "function"
array Array.isArray(a)
```

## Nested Partial

Use https://github.com/sindresorhus/type-fest -> PartialDeep

```ts
import { PartialDeep } from 'type-fest'

interface User {
  firstName: string;
  address: {
    city: string;
  };
}

const updateUser = (payload: PartialDeep<User>) => {};

updateUser({
  address: { city: 'Ponyville' },
});
```
