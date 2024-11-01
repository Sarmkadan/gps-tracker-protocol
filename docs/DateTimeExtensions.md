# DateTimeExtensions

A utility class providing common date/time conversions and manipulations for working with `DateTime` in GPS tracking scenarios, including Unix timestamps, rounding operations, day/month boundaries, relative time formatting, and ISO 8601 serialization.

## API

### `public static long ToUnixTimestamp(DateTime dateTime)`

Converts a `DateTime` to a Unix timestamp (seconds since 1970-01-01 00:00:00 UTC).

- **Parameters**
  - `dateTime`: The `DateTime` to convert. Must be in UTC or behavior is undefined.
- **Return value**
  - Unix timestamp as a `long` representing seconds since epoch.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `dateTime` is before 1970-01-01.

---

### `public static DateTime FromUnixTimestamp(long timestamp)`

Converts a Unix timestamp (seconds since 1970-01-01 00:00:00 UTC) to a `DateTime`.

- **Parameters**
  - `timestamp`: Unix timestamp in seconds.
- **Return value**
  - A `DateTime` in UTC representing the timestamp.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `timestamp` is negative or represents a date before 1970-01-01.

---

### `public static DateTime RoundDown(DateTime dateTime, long seconds)`

Rounds a `DateTime` down to the nearest whole second boundary.

- **Parameters**
  - `dateTime`: The `DateTime` to round.
  - `seconds`: The rounding interval in seconds. Must be positive.
- **Return value**
  - A `DateTime` rounded down to the nearest `seconds` interval.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `seconds` is less than or equal to zero.

---

### `public static DateTime RoundUp(DateTime dateTime, long seconds)`

Rounds a `DateTime` up to the nearest whole second boundary.

- **Parameters**
  - `dateTime`: The `DateTime` to round.
  - `seconds`: The rounding interval in seconds. Must be positive.
- **Return value**
  - A `DateTime` rounded up to the nearest `seconds` interval.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `seconds` is less than or equal to zero.

---
### `public static bool IsWithinSeconds(DateTime dateTime, DateTime other, long toleranceSeconds)`

Determines whether two `DateTime` values are within a specified tolerance in seconds.

- **Parameters**
  - `dateTime`: The reference `DateTime`.
  - `other`: The `DateTime` to compare.
  - `toleranceSeconds`: Maximum allowed difference in seconds. Must be non-negative.
- **Return value**
  - `true` if the absolute difference between the two dates is within `toleranceSeconds`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `toleranceSeconds` is negative.

---
### `public static string ToRelativeTime(DateTime dateTime)`

Generates a human-readable relative time string (e.g., "2 minutes ago", "in 3 hours").

- **Parameters**
  - `dateTime`: The `DateTime` to describe. Assumed to be in UTC or local context as appropriate.
- **Return value**
  - A localized or invariant string describing the relative time.
- **Exceptions**
  - None.

---
### `public static DateTime GetStartOfDay(DateTime dateTime)`

Returns a `DateTime` representing the start of the given day (00:00:00).

- **Parameters**
  - `dateTime`: Any `DateTime` value.
- **Return value**
  - A `DateTime` with time components set to midnight of the same day.
- **Exceptions**
  - None.

---
### `public static DateTime GetEndOfDay(DateTime dateTime)`

Returns a `DateTime` representing the end of the given day (23:59:59.999...).

- **Parameters**
  - `dateTime`: Any `DateTime` value.
- **Return value**
  - A `DateTime` with time components set to the last moment of the same day.
- **Exceptions**
  - None.

---
### `public static DateTime GetStartOfMonth(DateTime dateTime)`

Returns a `DateTime` representing the first moment of the month and year of the given date.

- **Parameters**
  - `dateTime`: Any `DateTime` value.
- **Return value**
  - A `DateTime` with day set to 1 and time set to midnight.
- **Exceptions**
  - None.

---
### `public static DateTime GetEndOfMonth(DateTime dateTime)`

Returns a `DateTime` representing the last moment of the month and year of the given date.

- **Parameters**
  - `dateTime`: Any `DateTime` value.
- **Return value**
  - A `DateTime` with day set to the last day of the month and time set to 23:59:59.999...
- **Exceptions**
  - None.

---
### `public static bool IsSameDay(DateTime dateTime, DateTime other)`

Determines whether two `DateTime` values fall on the same calendar day.

- **Parameters**
  - `dateTime`: The first `DateTime`.
  - `other`: The second `DateTime`.
- **Return value**
  - `true` if both dates represent the same day; otherwise, `false`.
- **Exceptions**
  - None.

---
### `public static string ToIso8601String(DateTime dateTime)`

Formats a `DateTime` as an ISO 8601 string in UTC (e.g., "2025-04-05T14:30:00Z").

- **Parameters**
  - `dateTime`: The `DateTime` to format.
- **Return value**
  - An ISO 8601-compliant string in UTC.
- **Exceptions**
  - None.

## Usage
