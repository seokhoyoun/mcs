## C# Coding Conventions

### Core Principles

* **Prioritize readability above all else.**
    * In most cases, the code itself should serve as its own documentation.
* **Follow the IDE's automatic formatting unless there is a specific reason not to.**
    * (e.g., Visual Studio's "Ctrl + K + D" function).

---

### Naming Conventions

1.  **Class and Struct Names:** Use **PascalCase**.
    * `class PlayerManager`
    * `struct PlayerData`
2.  **Local Variable and Function Parameter Names:** Use **camelCase**.
    * `public void SomeMethod(int someParameter)`
    * `int someNumber;`
3.  **Method Names:**
    * Follow a **verb (imperative) + noun (object)** format.
    * All method names use **PascalCase**.
    * For methods that simply return a **boolean** status, use **`Is`**, **`Can`**, **`Has`**, or **`Should`** as the verb prefix. If this feels unnatural, use another third-person singular verb that indicates a state.
        * `public bool IsAlive(Person person);`
        * `public bool HasChild(Person person);`
        * `public bool Exists(Person person);`
        * `public uint GetAge();`
4.  **Constant Names:**
    * Use **all uppercase** letters, with words separated by an **underscore (`_`)**.
    * `const int SOME_CONSTANT = 1;`
5.  **`static readonly` Variables (used as constants):**
    * Use **all uppercase** letters, with words separated by an **underscore (`_`)**.
    * `public static readonly MyConstClass MY_CONST_OBJECT = new MyConstClass();`
6.  **Namespace Names:** Use **PascalCase**.
    * `namespace System.Graphics`
7.  **`boolean` Variables:**
    * Prefix the variable name with **`b`**.
    * `bool bFired;`
8.  **`boolean` Properties:**
    * Prefix the name with **`Is`**, **`Has`**, **`Can`**, or **`Should`**.
    * `public bool IsFired { get; private set; }`
    * `public bool HasChild { get; private set; }`
9.  **Interface Names:**
    * Prefix the name with **`I`**.
    * `interface ISomeInterface`
10. **Enum Names:**
    * Prefix the name with **`E`**.
    * `public enum EDirection { North, South }`
11. **`private` Member Variables:**
    * Prefix the name with an **underscore (`_`)** and use **camelCase**.
    * `private readonly string _password;`
    * `private bool _bFired;`
12. **Value-Returning Function Names:**
    * Name them to clearly indicate what they return.
    * `public uint GetAge();`
13. **Loop Variable Names:**
    * For variables that are not simply used in a loop, avoid names like `i` or `e`. Instead, use descriptive names like `index` or `employee` that clearly identify the data being stored.
14. **Abbreviations:**
    * If an abbreviation is not followed by another word, use **all uppercase letters**.
    * `public int OrderID { get; private set; }`
    * `public int HttpCode { get; private set; }`
15. **Recursive Function Names:**
    * Append **`Recursive`** to the name.
    * `public void FibonacciRecursive();`
16. **`async` Method Names:**
    * Append the **`-Async`** suffix to the name.
17. **Nullable Parameters/Return Values:**
    * Append **`OrNull`** to the name.
    * `public Person GetEmployeeOrNull(int id);`

---

### Implementation Rules

1.  **`readonly` Variables:**
    * Declare variables as `readonly` if their value doesn't change after initialization.
    * `private readonly string _password;`
2.  **Properties:**
    * Use **properties** instead of separate getter and setter methods.
    * **Incorrect:** `public string GetName();` `public string SetName(string name);`
    * **Correct:** `public string Name { get; set; }`
3.  **Local Variable Declaration:**
    * Declare local variables on the same line where they are first used.
4.  **Floating-Point Values:**
    * Unless a `double` is strictly necessary, append **`f`** to floating-point values to declare them as `float`.
    * `float f = 0.5F;`
5.  **`switch` Statements:**
    * Always include a **`default:`** case.
6.  **`Debug.Assert()`:**
    * Use `Debug.Assert()` to enforce all assumptions made during development.
    * `Debug.Fail("unknown type");`
7.  **Function Overloading:**
    * Avoid function overloading when parameter types are generic.
    * **Correct:** `public Anim GetAnimByIndex(int index);` `public Anim GetAnimByName(string name);`
    * **Incorrect:** `public Anim GetAnim(int index);` `public Anim GetAnim(string name);`
    * **Prefer function overloading over default parameters.** If default parameters must be used, use values with a zero bit pattern like `null`, `false`, or `0`.
8.  **Collections:**
    * Always use containers from **`System.Collections.Generic`** instead of `System.Collections`. Using a pure array is also acceptable.
9.  **`var` Keyword:**
    * Try to avoid using the `var` keyword.
    * Exceptions are allowed when the data type is clearly visible on the right side of the assignment statement (e.g., `"string obviously"`, `new Employee()`) or when the data type is not critical.
10. **`async` Methods:**
    * Use **`async Task`** instead of `async void`. The only exception is for event handlers.
11. **Data Validation:**
    * Validate external data at the external/internal boundary. If there are issues, return before passing the data to an internal function.
    * This means all data that enters the internal system can be assumed to be valid.
12. **Exception Handling:**
    * Try not to throw exceptions from internal functions. Exceptions should be handled at the boundaries.
    * **Exception Allowed:** It is acceptable to throw an exception in a `default:` case within a `switch` statement on an `enum` to catch unhandled enum values.
13. **`null` Values:**
    * Strive to not allow `null` as a function parameter, especially for `public` functions.
    * Strive to not return `null` from a function, especially a `public` function.

---

### Structural Rules

1.  **Classes/Source Files:**
    * Each class should be in a separate source file.
    * A small exception is allowed for a few small, related classes that are logically grouped in a single file.
2.  **File Names:**
    * File names **must match the class name** exactly, including case.
    * `public class PlayerAnimation` -> `PlayerAnimation.cs`
3.  **`partial` Classes:**
    * The file name should start with the class name, followed by a period and a descriptive sub-name.
    * `public partial class Human` -> `Human.Head.cs`, `Human.Body.cs`
4.  **Member Order within a Class:**
    * The order of member variables and methods within a class should be:
        1.  **Properties** (the corresponding `private` member variable should be placed directly above the property).
        2.  **Member variables**.
        3.  **Constructors**.
        4.  **Methods** (in the order of public -> private).
5.  **Grouping:**
    * Group related methods and member variables together within the class.
6.  **Bit Flag Enums:**
    * Append **`Flags`** to the name.
    * `[Flags]`
    * `public enum EVisibilityFlags`
7.  **Variable Shadowing:**
    * **Variable shadowing is not allowed.** If an outer variable is using a name, use a different name for the inner variable.
8.  **Object Initializers:**
    * Try to avoid using object initializers.