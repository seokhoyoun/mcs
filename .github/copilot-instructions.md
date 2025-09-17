## C# Coding Conventions

### Core Principles

* **Prioritize readability above all else.**
    * In most cases, the code itself should serve as its own documentation.
* **Follow the IDE's automatic formatting unless there is a specific reason not to.**
    * (e.g., Visual Studio's "Ctrl + K + D" function).

---

### Domain Conventions (Nexus)

- Robot AMR port locations use the robot ID prefix.
  - Format: [RobotId].CP01 (e.g., RBT01.CP01), not AMR.CP01.
  - Always derive AMR marker/location IDs from the active robot context when creating plans or simulating.
  - Keep consistent across Portal, Orchestrator, and Infrastructure code.

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

# MCS-ACS  ��� ��缭 v1.4.1



[TOC]

## 1. ����

�� ������ MCS(Manufacturing Control System)�� **����**, ACS(Automatic Control System)�� **Ŭ���̾�Ʈ**�� �Ͽ� WebSocket ������� ����ϴ� �ý����� �⺻ ����� �����մϴ�.

---



## 2. �ý��� ����

- **MCS (����)**: �߾� ���� ���� �ý���, WebSocket ���� ���� ����

- **ACS (Ŭ���̾�Ʈ)**: �� ���� ���� �ý���, WebSocket Ŭ���̾�Ʈ�� MCS�� ����

  

---



## 3. ��� ����

| ���                          | ����                                                         | �ĺ��� ����      |
| ----------------------------- | ------------------------------------------------------------ | ---------------- |
| **����Ŀ(Stocker)**           | ���� ���� ī��Ʈ�� ������ �� �ִ� ��Ʈ(���ⱸ)�� ���� ���� (��Ʈ ���� ���� Ȯ��) | `ST01`, `ST02`   |
| **ī��Ʈ ��Ʈ(CP)**           | ī��Ʈ�� ���� �� �ִ� ��Ʈ                                   | `CP01`, `CP02`   |
| **Ʈ���� ��Ʈ(TP)**           | Ʈ���̸� ���� �� �ִ� ��Ʈ                                   | `TP01`, `TP15`   |
| **�޸� ��Ʈ(MP)**           | �÷Ḧ ���� �� �ִ� ��Ʈ                                     | `MP01`, `MP25`   |
| **�����(Area)**            | 6���� ī��Ʈ ���� ��Ʈ�� ���� ���� ���� (��Ʈ ����, ���� ���� Ȯ��) | `A01`, `A02`     |
| **��Ʈ(Set)**                 | 32���� �÷�(Memory) �׽�Ʈ�� ������ �˻��                   | `SET01`, `SET12` |
| **�����κ�(Logistics Robot)** | �� ���� ī��Ʈ ���� ��Ʈ�� ���� ���� ��� �κ�               |                  |
| **�۾��κ�(Control Robot)**   | ���� ���� Ʈ���̸� �ٷ� �� �ִ� ��Ʈ�� ���� �۾� ���� �κ�   |                  |



### 3.1 ��ġ���� ǥ��

**�ֿ� ��Ʈ/���Ժ� ��ġ���� ǥ��**

- **ī��Ʈ ��Ʈ(Cassette Port, CP)**

  - ����: ī��Ʈ�� ����/�̼��ϴ� ���ⱸ ��Ʈ
  - ǥ�� ����:  
    - �����1�� 2�� ī��Ʈ ��Ʈ �� `A01.CP02`
    - ����Ŀ2�� 5�� ī��Ʈ ��Ʈ �� `ST02.CP05`

- **Ʈ���� ��Ʈ(Tray Port, TP)**

  - ����: Ʈ���̸� ����/�̼��ϴ� ��Ʈ

  - ǥ�� ����:  

    - �۾��κ��� 3�� Ʈ���� ��Ʈ �� `AMR.TP03`

      

- **�޸� ��Ʈ(Memory Port, MP)**

  - ����: �޸�(�÷�)�� �����ϴ� ����(��Ʈ)
  - ǥ�� ����:  
    - ��Ʈ2�� 15�� �޸� ��Ʈ �� `SET02.MP15`

**��ġ����(��Ʈ, ��Ʈ, Ʈ���� ��)�� ��ȣ �ο� ��Ģ**

- **���� ���� ����(Top View)** �������� �Ѵ�.

- **���� ��(1��) �迭**
  �»�ܿ��� ���������� 1������ n������ ���� �ο� (��: `CP01`, `SET01`)
  
- **�ٴ�(2�� �̻�) �迭**
  ��� ����� �Ʒ���(Top to Bottom), �� ���� �������� ����(Left to Right)���� 1������ �ο�  
  (��: 2�� 3�� ��Ʈ�� ���)
  
  ```
  1��: SET01 SET02 SET03
  2��: SET04 SET05 SET06
  ```
  
- �� �ڸ� ���ڵ� �׻� 2�ڸ��� 0�� �е��Ͽ� ǥ���մϴ�. (��: `CP01`, `SET04`)

- �ٸ� ��ġ ��ҵ� ���� ��Ģ ����

- **���� ����**
    - �����1�� 2��° ī��Ʈ��Ʈ: `A01.CP02`
- ��ȣ �ο� ����� **���� ���� �� ��ý��� ȭ��� �ִ��� ��ġ��ŵ�ϴ�.**
- ������ ����(3�� �̻�, �ұ�Ģ �迭 ��)�� ���� ����/���� �߰� ����



> ��� ��ġ���� ǥ��� �� ��Ģ�� ���� �ϰ��� �ְ� �ο��մϴ�.



---



## 4. ��� ��������

- **��������**: WebSocket (ws://, wss://)
- **��� ����**:  
  - �� ACS(Ŭ���̾�Ʈ) ��Ʈ(�����κ�/�۾��κ�)�� MCS(����)�� ���������� ����  
  - ���� �� ����� �ǽð� �޽��� �ۼ��� ����

---



## 5. �⺻ ��� ����

1. **ACS(Ŭ���̾�Ʈ) ��Ʈ(�����κ�/�۾��κ�)**�� MCS(����)�� WebSocket ���� ��û
2. ���� ���� ��, ACS�� **�ʱ� ���� �޽���**�� ����
3. MCS�� ������ Ȯ���ϰ�, �ʿ�� �ʱ� ����/������ ����
4. ���� �ǽð� ������/���/�̺�Ʈ�� ��������� �ۼ���



### 5.1 MCS Request Commands (MCS -> ACS)

| Command                 | ����                                                       |
| ----------------------- | ---------------------------------------------------------- |
| `ExecutionPlan`         | Lot ���� �۾� �帧 ����. ���� Step ����                    |
| `CancelPlan`            | �۾� ��� ���� �÷� ��� ��û                              |
| `AbortPlan`             | ��� �ߴ� �� ���� ��û (��� �ߴ�)                         |
| `PausePlan`             | �۾� �Ͻ� ���� ��û                                        |
| `ResumePlan`            | �۾� �簳 ��û                                             |
| `SyncConfig`            | ���� ����, ��Ʈ, Ʈ���� ���� �� ���� ����                  |
| `RequestAcsPlans`       | ACS�� ���� �Ҵ� �� �÷� ��Ȳ ��û                          |
| `RequestAcsPlanHistory` | ACS���� �Ϸ� �� �÷� ��� ��ȸ (Offline to Online �� ���) |
| `RequestAcsErrorList`   | ACS�� ���� �߻� �� ����/�˶� ��û                          |



### 5.2 ACS Request Commands (ACS -> MCS)

| Command              | ����                                             |
| -------------------- | ------------------------------------------------ |
| `Registration`       | �ʱ� MCS ���� ���� �޼���                        |
| `PlanReport`         | Plan ���� ����                                   |
| `StepReport`         | Step ���� ����                                   |
| `JobReport`          | Job ���� ����                                    |
| `ErrorReport`        | ���� �Ǵ� ��� �̺�Ʈ �߻� ����                  |
| `RobotStatusUpdate`  | Robot�� ���� ���� ���� (�ֱ��� �Ǵ� �̺�Ʈ ���) |
| `TscStateUpdate`     | TSC ���� ���� �� ����                            |
| `CancelResultReport` | �۾� ��� ��û ��� ��ȯ                         |
| `AbortResultReport`  | ��� �ߴ� ��û ��� ��ȯ                         |
| `PauseResultReport`  | �Ͻ� ���� ��û ��� ��ȯ                         |
| `ResumeResultReport` | �۾� �簳 ��û ��� ��ȯ                         |
| `AcsCommStateUpdate` | ACS�� ���� ���� ����                             |



### 5.3 ACS Periodic Reporting Commands (ACS -> MCS)

| Command               | ����                                                         |
| --------------------- | ------------------------------------------------------------ |
| `RobotPositionUpdate` | �κ� ��ġ ������ 0.2(200msec)�ʸ��� �ֱ������� MCS�� �����մϴ�. |

---



## 6. �޽��� ����

- ��� �޽����� **JSON ����**�� ����ϸ�,  �޽��� Ÿ�� �� ������ ������ ������ �����ϴ�.
- ��� JSON �޽����� �ʵ���� camelCase(�ҹ��ڷ� ����, �ܾ� ���� �� �빮��)�� ǥ���մϴ�.



### 6.1 ���� �޽��� ����

```json
// ��� ��û(request) �޽����� �Ʒ��� ���� ���� ������ ���� JSON ������ ����Ѵ�.

{
  "command": "��ɾ� �Ǵ� �̺�Ʈ Ÿ��",
  "transactionId": "3f0f8b9e-095d-4dbf-bb13-1e8f4c1875a1",  // UUID (string),
  "timestamp": "2025-07-02T21:00:00.123+09:00", // ISO 8601 ������ ���� �ð�
  "payload": { 
    // ���/�̺�Ʈ�� �� ������ ��ü 
  }
}

```

```json
// ��� ����(response) �޽����� �Ʒ��� ���� ���� ������ ����Ѵ�.
// ��� Ŀ�ǵ�(command) ��û�� ����, ���� ���� ���� ����(����/����)�� ������� �ݵ�� ���� �޽���(ACK)�� ��ȯ�ؾ� �Ѵ�.

{
  "command": "��û�� ���� ���� ��ɾ�",
  "transactionId": "3f0f8b9e-095d-4dbf-bb13-1e8f4c1875a1",
  "timestamp": "2025-07-02T21:00:00.456+09:00",
  "result": "Success",   // "Success", "Fail" 
  "message": "����/���� �� ���� (�ʿ� ��)",
  "payload": {
    // ���亰 �� ������ ��ü (�ʿ� ��)
  }
}
```

`* ���ǵ��� ���� payload�� ���� �� ��� "result" : "Fail"`

### 6.2 �ʱ� ���� (Registration)

```json
// [ACS �� MCS] �ʱ� ����/�κ� ��� �޽��� ����
{
  "command": "Registration",
  "transactionId": "e8e497a9-03e9-4b52-bb9a-43c83deac3b4",
  "timestamp": "2025-07-02T18:30:00.000+09:00",
  "payload": {
    
  }
}

```

```json
// [MCS �� ACS] Registration ����(ACK) �޽���
{
  "command": "RegistrationAck",
  "transactionId": "d4f56d67-965e-42c1-b125-4e7be5e2a6a3", // ��û�� �����ϰ� ��ȯ
  "timestamp": "2025-07-02T18:30:01.200+09:00",
  "result": "Success",
  "message": "����� �Ϸ�Ǿ����ϴ�.",
  "payload": {}
}

```

```mermaid
sequenceDiagram
    participant LR_ACS as ACS(�κ� Ŭ���̾�Ʈ)
    participant MCS as MCS(����)

    rect rgb(230,247,255)
        note right of LR_ACS: �����κ� ACS<br/>
        LR_ACS->>MCS: Registration
        MCS-->>LR_ACS: RegistrationAck
    end



```

```mermaid
sequenceDiagram
    participant ACS as ACS(�κ� Ŭ���̾�Ʈ)
    participant MCS as MCS(����)

    ACS->>MCS: Registration 
    MCS-->>ACS: RegistrationAck

    Note over ACS,MCS: ?? �ʱ� ���� �� ACS���� <br>���� �������ִ� �÷� ��Ȳ ���� ������Ʈ

    MCS->>ACS: RequestAcsPlans
    ACS-->>MCS: RequestAcsPlansAck(plans)
    
  

```





### 6.3 ���� �κ� ���� ��� (ExecutionPlan)

| �׼Ǹ�         | ����                                                         | ���� from | ���� to  |
| -------------- | ------------------------------------------------------------ | --------- | -------- |
| CassetteLoad   | ī��Ʈ�� �����κ��� CP�� ����                                | ST01.CP02 | AMR.CP01 |
| CassetteUnload | �����κ����� ����Ŀ �Ǵ� ������� CP ������ ī��Ʈ ��ε�(�ݳ�) | AMR.CP01  | A02.CP04 |

```json
// [MCS �� ACS] ExecutionPlan �޽��� ���� 
{
  "command": "ExecutionPlan",
  "transactionId": "e2a97f63-4ed2-4d85-a2b3-11a51c188111",
  "timestamp": "2025-07-02T18:45:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-001", // �۾� ��ȹ(Plan) ����ID
    "lotId": "LOTSAMPLE_0001",     // LOT �ĺ���
    "priority": 10,                // (�ɼ�) �켱����
    "steps": [
      {
        "stepNo": 1,
        "action": "CassetteLoad",     // Ʈ���� ���� ��� ī��Ʈ ����
        "position": "A01.CP01",       // �����1�� ī��Ʈ��Ʈ1 (CP)
        "carrierIds": ["CASSETTE_01"],          
        "jobs": [
          {
            "jobId": "a4184b0d-bc13-4eb2-b9e2-2ab3a150a1c1",
            "from": "A01.CP01",        // ������ ī��Ʈ��Ʈ1 (CP)
            "to": "AMR.CP01"          // �����κ��� Cassette Port (����)
          }
        ]
      },
      {
        "stepNo": 2,
        "action": "CassetteUnload",   // ��ε�(�ݳ�) �۾�
        "position": "A02.CP03",       // �ݳ� ��� ��ġ (����: �����2, CP03)
        "carrierIds": ["CASSETTE_01"],          
        "jobs": [
          {
            "jobId": "b2dc9951-3e2e-44b2-b6e9-04ec4d09c013",
            "from": "AMR.CP01",
            "to": "A02.CP03"
          }
        ]
      }
    ]
  }
}

```

```json
// [ACS �� MCS] ExecutionPlanAck �޽��� ���� 

{
  "command": "ExecutionPlanAck", 
  "transactionId": "e2a97f63-4ed2-4d85-a2b3-11a51c188111", // ��û�� �����ϰ� ��ȯ
  "timestamp": "2025-07-02T18:45:01.025+09:00", 
  "result": "Success",          // "Success" �Ǵ� "Fail"
  "message": "Plan registered Successfully.", // ����/���� �޽���
  "payload": {
    "planId": "PLAN-20250702-001",  // ��ϵ�(Ȥ�� �źε�) Plan ID
    // �߰��� �ʿ��� ���� �����Ͱ� �ִٸ� ���⿡ ����
  }
}

```

```mermaid
sequenceDiagram
    participant MCS as MCS(����)
    participant ACS as ACS(�κ� Ŭ���̾�Ʈ)

    %% ExecutionPlan - ??
    Note over MCS,ACS: ?? ExecutionPlan 
    MCS->>ACS: ExecutionPlan
    ACS-->>MCS: ExecutionPlanAck

	loop 

    %% JobReport - ??
    Note over ACS,MCS: ?? JobReport 
    ACS-->>MCS: JobReport (Job 01 �Ϸ�)
    ACS-->>MCS: JobReport (Job 02 �Ϸ�)
    ACS-->>MCS: JobReport (Job 03 �Ϸ�)

    %% StepReport - ??
    Note over ACS,MCS: ?? StepReport 
    ACS-->>MCS: StepReport (Step 01 �Ϸ�)
    
	end


```

```apl
// MCS �� JobReport / StepReport�� Ack�� �� ���̾�׷������� �����Ǿ���. 
```



### 6.4 �۾� �κ� ���� ��� (ExecutionPlan)

| �׼Ǹ�             | ����                                        | ���� from     | ���� to        |
| ------------------ | ------------------------------------------- | ------------- | -------------- |
| TrayLoad           | Ʈ���̸� �۾��κ��� TP�� ����               | A01.CP01.TP01 | AMR.TP01       |
| MemoryPickAndPlace | �÷Ḧ Ʈ���� �� MP �� SET �� MP�� �̵�      | AMR.TP01.MP01 | A01.SET01.MP01 |
| TrayUnload         | �۾� �κ����� �ٸ� TP�� Ʈ���� ��ε�(�ݳ�) | AMR.TP03      | A01.CP01.TP01  |
| CloseSetCover      | ��Ʈ Ŀ�� Close                             |               |                |
| OpenSetCover       | ��Ʈ Ŀ�� Open                              |               |                |
| Start              | ���� ��ư Push                              |               |                |

```json
// [MCS �� ACS] ExecutionPlan �޽��� ����

{
  "command": "ExecutionPlan",
  "transactionId": "e2a97f63-4ed2-4d85-a2b3-11a51c188111",
  "timestamp": "2025-07-02T18:45:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-001", // �۾� ��ȹ(Plan) ����ID
    "lotId": "LOTSAMPLE_0001",             // LOT �ĺ���
    "priority": 10,                // (�ɼ�) �켱����
    "steps": [
      {
        "stepNo": 1, // 1�� step (�������)
        "action": "TrayLoad",
        "carrierIds": ["TRAY_01", "TRAY_02"],
        "position": "A01.CP01",
        "jobs": [
          {
            "jobId": "cd3a109a-8f19-4f19-86ea-552e2cb445f7",
            "from": "A01.CP01.TP01",
            "to": "AMR.TP01"
          },
          {
            "jobId": "b3bc9fc6-2603-4710-b9ed-6e228e99c1d2",
            "from": "A01.CP01.TP02",
            "to": "AMR.TP02"
          }
        ]
      },
      {
        "stepNo": 2,
        "action": "MemoryPickAndPlace",
        "position": "A01.SET01",
        "carrierIds": [], // MemoryPickAndPlace������ ������ ����.
        "jobs": [
          {
            "jobId": "f34c1ea4-0fa2-4c0f-9f2e-0702b2d2671d",
            "from": "AMR.TP01.MP01",
            "to": "A01.SET01.MP01"
          },
          {
            "jobId": "e0dc96b8-5f5a-4d8b-a7c1-18b29e13e53d",
            "from": "AMR.TP02.MP01",
            "to": "A01.SET01.MP02"
          }
        ]
      },
      {
        "stepNo": 3,
        "action": "TrayUnload",
        "position": "A01.CP01",
        "carrierIds": ["TRAY_01","TRAY_02"],          
        "jobs": [
          {
            "jobId": "aa0be8ca-5d22-4c3c-a671-932fd8c71f8b",
            "from": "AMR.TP01",
            "to": "A01.CP01.TP03"
          },
          {
            "jobId": "53b7155d-bffd-4b90-a72f-5e94fdf257e2",
            "from": "AMR.TP02",
            "to": "A01.CP01.TP04"
          }
        ]
      }
    ]
  }
}

```

```json
// [ACS �� MCS] ExecutionPlanAck �޽��� ���� 

{
  "command": "ExecutionPlanAck", 
  "transactionId": "e2a97f63-4ed2-4d85-a2b3-11a51c188111", // ��û�� �����ϰ� ��ȯ
  "timestamp": "2025-07-02T18:45:01.025+09:00", 
  "result": "Success",          // "Success" �Ǵ� "Fail"
  "message": "Plan registered Successfully.", // ����/���� �޽���
  "payload": {
    "planId": "PLAN-20250702-001",  // ��ϵ�(Ȥ�� �źε�) Plan ID
    // �߰��� �ʿ��� ���� �����Ͱ� �ִٸ� ���⿡ ����
  }
}

```

```mermaid
sequenceDiagram
    participant MCS as MCS(����)
    participant ACS as ACS(�κ� Ŭ���̾�Ʈ)

    Note over MCS,ACS: ?? ExecutionPlan : �۾� ��ȹ ���� (planId)

    alt ����� �����ϴ� ���
        MCS->>ACS: ?? ExecutionPlan(planId)
        ACS-->>MCS: ?? ExecutionPlanAck(result: Success, message: "Plan accepted")
        Note right of ACS: Plan ���� ���, ť�� �߰�
    else ������(Abnormal) - ��� �ź�/����
        MCS->>ACS: ?? ExecutionPlan(planId)
        ACS-->>MCS: ?? ExecutionPlanAck(result: Fail, message: "Queue Full or Duplicated Plan")
        Note right of ACS: Plan ��� ����, ���� ���� 
    end

```



### 6.5 �۾� ��� (CancelPlan) 

```json
// [MCS �� ACS] ���� �� �۾�(Plan) �ߴ� ��û(CancelPlan)
// AMR ���� �� ���� ��� ����
{
  "command": "CancelPlan",
  "transactionId": "4f9a5e50-8b6f-4f0d-b41f-38791bc3ee8a",
  "timestamp": "2025-07-02T18:50:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-002",
    "reason": "User request"
  }
}

```

```json
// [ACS �� MCS] �۾� �ߴ� ��û�� ���� ����(CancelPlanAck)
{
  "command": "CancelPlanAck",
  "transactionId": "4f9a5e50-8b6f-4f0d-b41f-38791bc3ee8a",
  "timestamp": "2025-07-02T18:50:00.500+09:00",
  "result": "Success",
  "message": "Plan cancelled Successfully.",
  "payload": {
    "planId": "PLAN-20250702-002"
  }
}
```

```mermaid
sequenceDiagram
    participant MCS as MCS (����)
    participant ACS as ACS (�κ� Ŭ���̾�Ʈ)

    Note over MCS,ACS: ?? Plan ��� ����

    MCS->>ACS: ?? CancelPlan(planId)
    ACS-->>MCS: ?? CancelPlanAck(result: Success)

    Note right of ACS: Cancel ���� ���� �Ǵ� (ACS)

    ACS-->>MCS: ?? CancelResultReport(planId, status: Success)
    MCS-->>ACS: ?? CancelResultReportAck

```

#### 6.5.A �۾� ��� ��� ���� (CancelResultReport) 

```json
// [ACS �� MCS] CancelPlan ��û ���� ���� Plan ���� ����
{
  "command": "CancelResultReport",
  "transactionId": "a83f4e2b-d54a-4baf-9b88-fc888d7d4321",
  "timestamp": "2025-08-07T10:22:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-002",
    "result": "Success", // ���� Cancel �Ϸ�Ǹ� "Success"
    "message": "Plan has been successfully cancelled."
  }
}

```

```json
// [MCS �� ACS] CancelResultReport�� ���� ����(CancelResultReportAck)
{
  "command": "CancelResultReportAck",
  "transactionId": "a83f4e2b-d54a-4baf-9b88-fc888d7d4321",
  "timestamp": "2025-08-07T10:22:00.150+09:00",
  "result": "Success",
  "message": "Cancellation result received.",
  "payload": {}
}

```

#### Abnormal Case 01. �۾� �� CancelPlan ��û ��,

- �÷��� �̹� �۾� �� `CancelPlan` �߻��� ���,  `CancelResultReport`�� ������ `Failed` ó��

  



### 6.6 ��� �ߴ� (AbortPlan) 

```json
// [MCS �� ACS] ��� ���� �ߴ� ��û(AbortPlan)
{
  "command": "AbortPlan",
  "transactionId": "ee327ea6-845a-4fd5-ae89-96011e69a6df",
  "timestamp": "2025-07-02T18:51:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-002",
    "reason": "Emergency stop triggered"
  }
}

```

```json
// [ACS �� MCS] ��� �ߴ� ��û�� ���� ����(AbortPlanAck)
{
  "command": "AbortPlanAck",
  "transactionId": "ee327ea6-845a-4fd5-ae89-96011e69a6df",
  "timestamp": "2025-07-02T18:51:00.200+09:00",
  "result": "Success",
  "message": "Plan aborted immediately.",
  "payload": {
    "planId": "PLAN-20250702-002"
  }
}
```



#### 6.6.A Abort ��� ���� (AbortResultReport)

```json
// [ACS �� MCS] AbortPlan ��û ó�� ��� ���� 
{
  "command": "AbortResultReport",
  "transactionId": "7adcf771-3b6e-4de2-90a5-d91d23f14152",
  "timestamp": "2025-08-07T10:30:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-002",
    "result": "Success", 
    "message": "Plan aborted."
  }
}

```

```json
// [MCS �� ACS] AbortResultReportAck
{
  "command": "AbortResultReportAck",
  "transactionId": "7adcf771-3b6e-4de2-90a5-d91d23f14152",
  "timestamp": "2025-08-07T10:30:00.150+09:00",
  "result": "Success",
  "message": "Abort result received.",
  "payload": {}
}
```



### 6.7 �۾� �Ͻ� ���� (PausePlan)

```json
// [MCS �� ACS] �۾� �Ͻ� ���� ��û(PausePlan)
{
  "command": "PausePlan",
  "transactionId": "8bca2d62-df39-4c60-9c4c-57f4bdf6e09f",
  "timestamp": "2025-07-02T18:52:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-002",
    "reason": "Temporary stop for inspection"
  }
}

```

```json
// [ACS �� MCS] �۾� �Ͻ� ���� ��û�� ���� ����(PausePlanAck)
{
  "command": "PausePlanAck",
  "transactionId": "8bca2d62-df39-4c60-9c4c-57f4bdf6e09f",
  "timestamp": "2025-07-02T18:52:00.300+09:00",
  "result": "Success",
  "message": "Plan paused.",
  "payload": {
    "planId": "PLAN-20250702-002"
  }
}
```

#### 6.7.A Pause ��� ���� (PauseResultReport)

```json
// [ACS �� MCS] PausePlan ��û ó�� ��� ���� 
{
  "command": "PauseResultReport",
  "transactionId": "3b8c1294-6ac6-4a85-9565-e4624aa44b82",
  "timestamp": "2025-08-07T10:31:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-002",
    "result": "Success", 
    "message": "Plan paused successfully."
  }
}

```

```json
// [MCS �� ACS] PauseResultReportAck
{
  "command": "PauseResultReportAck",
  "transactionId": "3b8c1294-6ac6-4a85-9565-e4624aa44b82",
  "timestamp": "2025-08-07T10:31:00.150+09:00",
  "result": "Success",
  "message": "Pause result received.",
  "payload": {}
}

```

?? Plan�� ������ `Paused` ���·� ��ȯ�� ���, ACS�� �Ʒ� �޽����� **�ʼ������� ����**�ؾ� �մϴ�:

- `RobotStatusUpdate`: �κ��� ���� �ߴ� ���� ���� 
- `PlanReport(status: Paused)`: Plan ���� ���� ����
- `PauseResultReport`: Pause ���� ��� ����

```mermaid
sequenceDiagram
    participant MCS as MCS (����)
    participant ACS as ACS (�κ� Ŭ���̾�Ʈ)

    MCS->>ACS: ?? PausePlan(planId)
    ACS-->>MCS: PausePlanAck    

    Note right of ACS: ���� �۾� ���� �� �κ� ���� ���� ����

    ACS-->>MCS: ?? RobotStatusUpdate(robotStatus: Stopped)
    ACS-->>MCS: ?? PlanReport(status: Paused)
    ACS-->>MCS: ?? PauseResultReport(status: Paused)
    MCS->>ACS: ?? PauseResultReportAck

```





### 6.8 �۾� �簳 (ResumePlan)

```json
// [MCS �� ACS] �۾� �簳 ��û(ResumePlan)
{
  "command": "ResumePlan",
  "transactionId": "e0b6c644-2851-4b17-853e-6766f6e81f1b",
  "timestamp": "2025-07-02T18:53:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-002"
  }
}

```

```json
// [ACS �� MCS] �۾� �簳 ��û�� ���� ����(ResumePlanAck)
{
  "command": "ResumePlanAck",
  "transactionId": "e0b6c644-2851-4b17-853e-6766f6e81f1b",
  "timestamp": "2025-07-02T18:53:00.150+09:00",
  "result": "Success",
  "message": "Plan resumed.",
  "payload": {
    "planId": "PLAN-20250702-002"
  }
}
```

#### 6.8.A Resume ��� ���� (ResumeResultReport)

```json
// [ACS �� MCS] ResumePlan ��û ó�� ��� ���� (���� �簳 �Ϸ� ����)
{
  "command": "ResumeResultReport",
  "transactionId": "fbf125f4-d8d1-49c1-802d-dbd22728c55b",
  "timestamp": "2025-08-07T10:32:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-002",
    "result": "Success", 
    "message": "Plan resumed successfully."
  }
}

```

```json
// [MCS �� ACS] ResumeResultReportAck
{
  "command": "ResumeResultReportAck",
  "transactionId": "fbf125f4-d8d1-49c1-802d-dbd22728c55b",
  "timestamp": "2025-08-07T10:32:00.150+09:00",
  "result": "Success",
  "message": "Resume result received.",
  "payload": {}
}

```





### 6.9 ~~���� ����ȭ (SyncConfig) <span style="color: red;">??</span>~~

```json
// [MCS �� ACS] ���� ���� ����ȭ ��û(SyncConfig)
// ��� �Ӽ� ���� ���� ��..

```

```json
// [ACS �� MCS] ���� ����ȭ ��û�� ���� ����(SyncConfigAck)
{
  "command": "SyncConfigAck",
  "transactionId": "6c1e4b86-69a1-4e1f-bc93-4e6c1ef4de0c",
  "timestamp": "2025-07-02T18:54:00.200+09:00",
  "result": "Success",
  "message": "Config synced Successfully.",
  "payload": {}
}
```



### 6.10 �÷� ��Ȳ ��û (RequestAcsPlans)

| �÷� ����  | ����                                     |
| ---------- | ---------------------------------------- |
| Pending    | ���� ��� ��. ���� ���۵��� ����         |
| InProgress | ���� ���� ���� Step�� ����               |
| Paused     | �ܺ� ���� �Ǵ� ���� �������� �Ͻ� �ߴܵ� |

```json
// [MCS �� ACS] ACS�� ���� �Ҵ� �� �÷� ��Ȳ ����
{
  "command": "RequestAcsPlans",
  "transactionId": "e731223b-b1a6-4e0d-8e7c-f8c8774a0fa7",
  "timestamp": "2025-07-02T18:55:00.000+09:00",
  "payload": {
  }
}

```

```json
// [ACS �� MCS] ���� ��û�� ���� ����(RequestAcsStatusAck)
{
  "command": "RequestAcsPlansAck",
  "transactionId": "a211ba25-24e2-47c2-bda2-2d8e3a1bbd77",
  "timestamp": "2025-07-03T11:22:00.035+09:00",
  "result": "success",
  "message": "ACS status returned.",
  "payload": { 
    "plans": [
      {
        "planId": "PLAN-20250703-200",
        "robotId": "CR01",
        "status": "Pending",       // Pending, InProgress, Paused
        "stepNo": 1,
        "jobId": "1311ba25-24e2-47c2-bda2-2d8e3a1b8412",
        "currentAction": "TrayLoad",
        "startTime": "2025-07-03T11:18:00.000+09:00",
        "endTime": null
      },
      {
        "planId": "PLAN-20250703-201",
        "robotId": "CR02",
        "status": "InProgress",    // (���� ����)
        "stepNo": 2,
        "jobId": "4311ba25-24e2-47ab-bda2-2d8e3a1b5687",
        "currentAction": "MemoryPickAndPlace",
        "startTime": "2025-07-03T11:20:00.000+09:00",
        "endTime": null
      }
    ]
  }
}
```

```mermaid
sequenceDiagram
    participant MCS as MCS(����)
    participant ACS as ACS(Ŭ���̾�Ʈ)

    Note over MCS,ACS: ?? RequestAcsStatus : ACS ���� �� �÷� ��Ȳ �ǽð� ����

   
        MCS->>ACS: ?? RequestAcsStatus()
        ACS-->>MCS: ?? RequestAcsStatusAck(connectedRobots, plans)
        Note right of ACS: ���� ����� CR ��� �� �� Plan�� ���� ����(Pending, InProgress ��) ��ȯ
  
  

```

* **status�� `Pending`�� ���** `stepNo`�� `jobId`�� �÷��� **ù ��° Step�� �ش� Step�� ù ��° Job**�� ǥ���մϴ�.

* **status�� `InProgress`�� ���** `stepNo`�� `jobId`�� **���� ���� Step (InProgress)�� ���� ���� Job (InProgress)**�� ǥ���մϴ�.

* **status�� `Paused`�� ���** `stepNo`�� **���� ���̴� Step (InProgress)**, `jobId`�� **�Ͻ� ���� (Paused)�� Job**�� ǥ���մϴ�.



### 6.11 ACS �÷� �̷� ��ȸ (RequestAcsPlanHistory)

| �÷� ����  | ����                                                         |
| ---------- | ------------------------------------------------------------ |
| Pending    | ���� ��� ��. ���� ���۵��� ����                             |
| InProgress | ���� ���� ���� Step�� ����                                   |
| Paused     | �ܺ� ���� �Ǵ� ���� �������� �Ͻ� �ߴܵ�                     |
| Completed  | ��� Step�� ���������� �Ϸ��                                |
| Failed     | �ϳ� �̻��� Step���� ������ �߻��Ͽ� Plan ��ü�� ����        |
| Cancelled  | �ܺο��� �۾��� ��ҵ� (�����/���� �ý��� ��)               |
| Aborted    | ���/������ ��Ȳ���� ��� �ߴܵ� (��� �ߴ�, �ý��� ��ȣ ��) |

```json
// [MCS �� ACS] Ư�� Plan ID(��)�� �Ϸ�� �÷� ��� �̷� ��ȸ
{
  "command": "RequestAcsPlanHistory",
  "transactionId": "fbc1b890-b173-4f71-b4d8-093e8d8a8f73",
  "timestamp": "2025-07-03T12:10:00.000+09:00",
  "payload": {
    "planIds": [
      "PLAN-20250701-051",
      "PLAN-20250701-052"
    ]
  }
}


```

```json
// [ACS �� MCS] �ش� planId�� �Ϸ� �̷�(�迭) ����

{
  "command": "RequestAcsPlanHistoryAck",
  "transactionId": "fbc1b890-b173-4f71-b4d8-093e8d8a8f73",
  "timestamp": "2025-07-03T12:10:00.030+09:00",
  "result": "Success",
  "message": "",
  "payload": {
    "plans": [
      {
        "planId": "PLAN-20250701-051",
        "robotId": "CR01",
        "status": "Completed",       
        "stepNo": 0,
        "jobId": null,
        "currentAction": null,
        "startTime": "2025-07-03T11:18:00.000+09:00",
        "endTime": "2025-07-03T12:35:20.000+09:00"
      },
      {
        "planId": "PLAN-20250701-052",
        "robotId": "CR01",
        "status": "Failed",    
        "stepNo": 2, 
        "jobId": "4311ba25-24e2-47ab-bda2-2d8e3a1b5687",
        "currentAction": "MemoryPickAndPlace",
        "startTime": "2025-07-03T11:20:00.000+09:00",
        "endTime": "2025-07-03T11:55:10.000+09:00"
      }
    ]
  }
}

```

```mermaid
sequenceDiagram
    participant MCS as MCS (����)
    participant ACS as ACS (�κ� Ŭ���̾�Ʈ)

    Note over MCS,ACS: ?? RequestAcsPlanHistory : Ư�� planId�� �Ϸ� �̷� ��ȸ

    MCS->>ACS: ?? RequestAcsPlanHistory(planIds)
    ACS-->>MCS: ?? RequestAcsPlanHistoryAck(plans)

```

* **status**�� **` Completed`�� ��** `stepNo`�� 0, `jobId`�� `null`�� ǥ���մϴ�.

* **status�� `Failed`�� ���** `stepNo`�� `jobId`�� **���а� �߻��� �ܰ�(step)�� �ش� �۾�(job)�� �ĺ���**�� ǥ���մϴ�. 

### 6.12 Plan ���� ���� (PlanReport)

| �÷� ����  | ����                                                         |
| ---------- | ------------------------------------------------------------ |
| Pending    | ���� ��� ��. ���� ���۵��� ����                             |
| InProgress | ���� ���� ���� Step�� ����                                   |
| Paused     | �ܺ� ���� �Ǵ� ���� �������� �Ͻ� �ߴܵ�                     |
| Completed  | ��� Step�� ���������� �Ϸ��                                |
| Failed     | �ϳ� �̻��� Step���� ������ �߻��Ͽ� Plan ��ü�� ����        |
| Cancelled  | �ܺο��� �۾��� ��ҵ� (�����/���� �ý��� ��)               |
| Aborted    | ���/������ ��Ȳ���� ��� �ߴܵ� (��� �ߴ�, �ý��� ��ȣ ��) |

```json
// [ACS �� MCS] Plan ���� ���� �� ����(PlanReport)
{
  "command": "PlanReport",
  "transactionId": "f7e1b865-552f-4d18-8d0c-cd330f2821f7",
  "timestamp": "2025-07-03T11:45:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250703-200",
    "status": "Completed",   
    "message": "Plan ��� Step�� ���� �Ϸ��.",
  }
}

```

```json
// [MCS �� ACS] PlanReportAck (����/������)
{
  "command": "PlanReportAck",
  "transactionId": "f7e1b865-552f-4d18-8d0c-cd330f2821f7",
  "timestamp": "2025-07-03T11:45:00.050+09:00",
  "result": "Success", // �Ǵ� "fail"
  "message": "PlanReport received.",
  "payload": {}
}
```

```mermaid
sequenceDiagram
    participant ACS as ACS(�κ� Ŭ���̾�Ʈ)
    participant MCS as MCS(����)

    Note over ACS,MCS: ?? PlanReport : �÷� ���� ���� �� MCS�� ����

    ACS-->>MCS: ?? PlanReport(status: Completed/Failed/Cancelled/Aborted)
    MCS-->>ACS: ?? PlanReportAck(result: success)
    Note right of MCS: PlanReport ���� ����/ó��
   

```





### 6.13 Step ���� ���� (StepReport)

| ����       | ����                                                         |
| ---------- | ------------------------------------------------------------ |
| Pending    | ���� Step�� �Ϸ�� ������ ���(Ready ����), ��⿭�� ����    |
| Dispatched | �ش� Step�� AMR �Ǵ� Robot���� ����(�Ҵ�)�� ����             |
| InProgress | �ش� Step�� ������ ����(����) �� (�̵�, ��/�÷��̽� �� ����) |
| Completed  | Step �� ��� Job�� ���������� �Ϸ�� ����                    |
| Failed     | �ϳ� �̻��� Job�� �����Ͽ� Step ��ü�� ���� ó����           |
| Skipped    | ���� �б�, ���� ó��, ���� ���۷��̼� ������ �ǳʶ�          |

```json
// [ACS �� MCS] Step ���� ����(StepReport)
{
  "command": "StepReport",
  "transactionId": "7b591b54-6f8d-47cc-9e39-6ed2a88f7a9d",
  "timestamp": "2025-07-02T21:00:00.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-010",
    "robotId": "CR01",
    "stepNo": 2,
    "status": "Completed",  // Pending, Dispatched, InProgress, Completed, Failed, Skipped
    "message": "Step executed Successfully."
  }
}

```

```json
// [MCS �� ACS] Step ���� ���� ���� ����(StepReportAck)
{
  "command": "StepReportAck",
  "transactionId": "7b591b54-6f8d-47cc-9e39-6ed2a88f7a9d",
  "timestamp": "2025-07-02T21:00:00.100+09:00",
  "result": "Success",
  "message": "Step report received.",
  "payload": {}
}
```

```mermaid
sequenceDiagram
    participant ACS as ACS(Ŭ���̾�Ʈ)
    participant MCS as MCS(����)

    Note over ACS,MCS: ?? StepReport : Step ���� ���� �� 

   
    ACS-->>MCS: ?? StepReport(stepNo, status)
   

```



### 6.14 Job ���� ���� (JobReport)

| ����         | ����                                       |
| ------------ | ------------------------------------------ |
| Pending      | ���� �κ����� ���޵��� ����                |
| Instructed   | �κ����� ��� ���� �Ϸ�                    |
| InProgress   | �κ��� �ش� �۾��� ���� ��                 |
| Completed    | �۾��� ���������� �Ϸ��                   |
| Failed       | �۾� ���� ���� �߻�                        |
| ~~Retrying~~ | ~~������ �۾��� ��õ� ��~~                |
| ~~Timeout~~  | ~~���� �ð� �� �Ϸ���� �ʾ� ���� ó����~~ |

```json
// [ACS �� MCS] Job ���� ����(JobReport)
{
  "command": "JobReport",
  "transactionId": "0a62528f-0b32-4c15-b45e-60b3e1ef9b6f",
  "timestamp": "2025-07-02T21:02:15.000+09:00",
  "payload": {
    "planId": "PLAN-20250702-010",
    "robotId": "CR01",
    "stepNo": 2,
    "jobId": "fa6bdb41-60ee-4b52-8c6f-8a6222b2b5c1",
    "status": "inProgress", // Pending, Instructed, InProgress, Completed, Failed, Timeout
    "message": "Job picking started."
  }
}

```

```json
// [MCS �� ACS] Job ���� ���� ���� ����(JobReportAck)
{
  "command": "JobReportAck",
  "transactionId": "0a62528f-0b32-4c15-b45e-60b3e1ef9b6f",
  "timestamp": "2025-07-02T21:02:15.050+09:00",
  "result": "Success",
  "message": "Job report received.",
  "payload": {}
}
```



```mermaid
sequenceDiagram
    participant ACS as ACS (�κ� Ŭ���̾�Ʈ)
    participant MCS as MCS (����)

    Note over ACS,MCS: ?? JobReport : ���� �۾�(Job)�� <br>���¸� MCS�� ����

   
    ACS-->>MCS: ?? JobReport
    Note right of ACS: Job ���� ���� �� 
 

```

#### Abnormal Case 01. Job Failed

- �۾� �� �ϳ� �̻��� Job���� `Failed` ���°� �߻��� ���, �ش� Step�� ��� `Failed` ���·� �����  
- Step�� �����ϸ�, �ش� Step�� ���Ե� ��ü Plan�� `Failed` ���·� ��� ��ȯ  `StepReport / PlanReport (status: Failed)`
- ���� ������� ���� ������ Step �� Job�� **���� ���� ���� ����** **�ߴ� ó��**
- ��ġ ��  **���ο� Plan���� ������**

```mermaid
sequenceDiagram
    participant ACS as ACS(�κ� Ŭ���̾�Ʈ)
    participant MCS as MCS(����)

    Note over ACS,MCS: ?? Job 3 ���� �� Step ���� �� Plan ���� ó��

    ACS-->>MCS: ?? JobReport(jobId: ..., status: Failed)
    ACS-->>MCS: ?? StepReport(stepNo: ..., status: Failed)
    ACS-->>MCS: ?? PlanReport(planId: ..., status: Failed)

```

#### Abnormal Case 02. Plan Aborted

- MCS�κ��� `AbortPlan` ����� ������ ���, Ư�� ������ ���� ���, �ش� Plan�� ��� **Aborted ����**�� ��ȯ.
- ��, **�̹� InProgress ���·� ���� ���� Job**�� �����ϴ� ��쿡��,
  �� �ش� Job�� �ߴ����� �ʰ� **���� �Ϸ�� ������ ����**�� �����
- InProgress Job�� ��� �Ϸ�� ����, ACS�� Plan ���¸� `Aborted`�� ������
- **������ ���� ������� ���� Step �� Job(Pending ����)**�� ���ؼ��� **������ ���� ���� ���� �ߴ�**
- �ߴܵ� Job/Step�� ���� `JobReport`, `StepReport` ���� **�۽����� ����**
- ���������� ���� �޽����� �����:
  1. `JobReport(status: Completed)` ? �̹� ���� ���̴� Job�� �Ϸ�� ���
  2. `PlanReport(status: Aborted)` ? Plan ��ü �ߴ� �Ϸ� ����





### 6.15 ����/�˶� �̺�Ʈ ���� (ErrorReport) <span style="color: red;">*�� ���� �ʿ�</span>

```json
// [ACS �� MCS] ���� �Ǵ� ��� �̺�Ʈ �߻� ����(ErrorReport) 
{
  "command": "ErrorReport",
  "transactionId": "d6c22da8-98bc-4e76-bdd9-6c89e1e3c8fa",
  "timestamp": "2025-07-02T21:05:33.000+09:00",
  "payload": {
    "robotId": "CR01",
    "planId": "PLAN-20250702-010",
    "state" : true, // true�� ��� �߻�, false�� ��� ���� ���� 
    "stepNo": 2,
    "jobId": "fa6bdb41-60ee-4b52-8c6f-8a6222b2b5c1", // �ʿ��
    "errorCode": "���� ����",
    "level" : "heavy",		// "heavy", "light"
    "message": "Tray is not detected in port."
  }
}

```

```json
// [MCS �� ACS] ����/��� �̺�Ʈ ���� ���� ����(ErrorReportAck)
{
  "command": "ErrorReportAck",
  "transactionId": "d6c22da8-98bc-4e76-bdd9-6c89e1e3c8fa",
  "timestamp": "2025-07-02T21:05:33.060+09:00",
  "result": "Success",
  "message": "Error report received.",
  "payload": {}
}
```



### 6.16 �κ� ���� ���� ���� (RobotStatusUpdate)

| robotStatus  | �ǹ�                       |
| ------------ | -------------------------- |
| Init         | �ʱ�ȭ ��                  |
| Disconnected | ��� ����                  |
| Manual       | ���� ���� ����             |
| Idle         | ���� ���� (�Ҵ� �۾� ����) |
| Moving       | �̵� ��                    |
| Arrival      | ������/��Ʈ ������         |
| Docking      | ��Ʈ�� ��ŷ ��             |
| UnDocking    | ��ŷ ���� ��               |
| Working      | �κ� �۾� ���� ��          |
| Charging     | ���� ��                    |
| Error        | ��� �Ǵ� �̻� �߻�        |
| Stopped      | �ܺ� ���� �Ǵ� ���� ����   |

```json
// [ACS �� MCS] robot status,position �Ǵ� Port ���� ���� ���� �κ� ���°� ���� �� ���,

// �����κ��� ���� ��ȭ
{
  "command": "RobotStatusUpdate",
  "transactionId": "b332fcbe-8916-4cd8-9ebc-2d6c76082b2c",
  "timestamp": "2025-07-02T22:36:00.000+09:00",
  "payload": {
    "robotId": "LR01",
    "robotType": "LR",          // ���� �κ��� ��� "LR"   
    "robotStatus": "Docking",
    "position": "A01.CP03",
    "carrierIds": ["CASSETTE_01"],
    "planId": null,
    "stepNo": null,
    "jobId": null,
    "message": "��Ʈ ��ŷ ��"
  }
}

// �۾��κ�
{
  "command": "RobotStatusUpdate",
  "transactionId": "d322ff09-23b8-4e3a-8c0a-ff1cd7c8123a",
  "timestamp": "2025-07-02T22:38:00.000+09:00",
  "payload": {
    "robotId": "CR01",
    "robotType": "CR",              // �۾� �κ��� ��� "CR"   
    "robotStatus": "Working",
    "position": "A01.SET03",
    "carrierIds": ["TRAY_01", "TRAY_02", "TRAY_03"],
    "planId": "PLAN-20250703-110",
    "stepNo": 2,
    "jobId": "d3324f09-00b8-1e3a-2c0a-aa1cd7c8123a3",
    "message": "Ʈ���� �ڵ鸵 �۾� ��"
  }
}


// �۾��κ� (carrierIds TP02/TP03�� �����ϴ� ���) [v1.2.0 ���� �߰�]
{
  "command": "RobotStatusUpdate",
  "transactionId": "d322ff09-23b8-4e3a-8c0a-ff1cd7c8123a",
  "timestamp": "2025-07-02T22:38:00.000+09:00",
  "payload": {
    "robotId": "CR01",
    "robotType": "CR",   
    "robotStatus": "Working",
    "position": "A01.SET03",
    "carrierIds": [null, "TRAY_02", "TRAY_03"], // Ʈ������Ʈ(TP)�� ����ִ� ��� null�� ����. 
    "planId": "PLAN-20250703-110",
    "stepNo": 2,
    "jobId": "d3324f09-00b8-1e3a-2c0a-aa1cd7c8123a3",
    "message": "Ʈ���� �ڵ鸵 �۾� ��"
  }
}
```

```json
// [MCS �� ACS] RobotStatusUpdate�� ���� ���� (RobotStatusUpdateAck)
{
  "command": "RobotStatusUpdateAck",
  "transactionId": "fda3335e-013b-4b0a-b01b-ec01a0cfa399",
  "timestamp": "2025-07-02T22:42:10.030+09:00",
  "result": "Success",
  "message": "Status update received.",
  "payload": {}
}
```

```mermaid
sequenceDiagram
    participant ACS as ACS(�κ� Ŭ���̾�Ʈ)
    participant MCS as MCS(����)

    Note over ACS,MCS: ?? RobotStatusUpdate : �κ� ���°� ���� �� ��� MCS�� ��� ����
    
    ACS-->>MCS: ?? RobotStatusUpdate(robotId, robotStatus: Arrival �� Docking)
    ACS-->>MCS: ?? RobotStatusUpdate(robotId, position: A01.SET02 �� A01.SET03)
    ACS-->>MCS: ?? RobotStatusUpdate(robotId, hasTray: false �� true)
    ACS-->>MCS: ?? RobotStatusUpdate(robotId, robotStatus: Alarm)
   

    Note right of ACS: ���°�(robotStatus, position) ��ȭ����<br>���/�̺�Ʈ�� �۽�

```

```apl
// MCS �� JobReport / StepReport�� Ack�� �� ���̾�׷������� �����Ǿ���. 
```



### 6.17 ����/�˶� ��Ȳ ��û (RequestAcsErrorList)

```json
// [MCS �� ACS] ACS�� ���� �߻� �� ����/�˶� ��Ȳ ����
{
  "command": "RequestAcsErrorList",
  "transactionId": "e731223b-b1a6-4e0d-8e7c-f8c8774a0fa7",
  "timestamp": "2025-07-02T18:55:00.000+09:00",
  "payload": {
   }
}

```

```json
// [ACS �� MCS] ���� ��û�� ���� ����(RequestAcsStatusAck)
{
  "command": "RequestAcsErrorListAck",
  "transactionId": "a211ba25-24e2-47c2-bda2-2d8e3a1bbd77",
  "timestamp": "2025-07-03T11:22:00.035+09:00",
  "result": "Success",
  "message": "ACS alarms returned.",
  "payload": { 
    "errors": [
     {
	    "robotId": "CR01",
    	"planId": "PLAN-20250702-010",
	    "state" : true, // true�� ��� �߻�, false�� ��� ���� ���� 
    	"stepNo": 2,
	    "jobId": "fa6bdb41-60ee-4b52-8c6f-8a6222b2b5c1", // �ʿ��
    	"errorCode": "���� ����",
	    "level" : "heavy",		// "heavy", "light"
    	"message": "Tray is not detected in port."
	  },
      {
	    "robotId": "CR01",
	    "state" : true, // true�� ��� �߻�, false�� ��� ���� ���� 
    	"errorCode": "���� ����",
	    "level" : "light",		// "heavy", "light"
    	"message": "low battery"
	  },
    ]
  }
} 
```



### 6.19 �κ� ��ġ ���� ���� (RobotPositionUpdate)

```json
{
  "command": "RobotPositionUpdate",
  "transactionId": "b332fcbe-8916-4cd8-9ebc-2d6c76082b2c",
  "timestamp": "2025-07-02T22:36:00.000+09:00",
  "payload": {
    "robots": [
      {
        "robotId": "LR01",
        "x": 10.24,
        "y": 15.87,
        "angle": 90,
        "battery": 75
      },
      {
        "robotId": "CR01",
        "x": 12.11,
        "y": 8.45,
        "angle": 45,
        "battery": 64
      }
    ]
  }
}

```

| �ʵ��  | Ÿ��   | ����      |
| ------- | ------ | --------- |
| robotId | string | �κ� ID   |
| x       | number | X ��ǥ    |
| y       | number | Y ��ǥ    |
| angle   | number | ����      |
| battery | number | ���͸�(%) |

> ?? RobotPositionUpdate �޽����� 200�и���(0.2��) �������� ��� ������Ʈ�˴ϴ�.
>
> ?? RobotPositionUpdate �޽����� ���� ������ ������ Ack(Ȯ��) �޽����� �������� �ʽ��ϴ�.



### 6.20 TSC ���� ���� (TscStateUpdate)

```json
// [ACS �� MCS] ���� TSC ���� ���� ��
{
  "command": "TscStateUpdate",
  "transactionId": "9f13f236-2c4b-42af-b94b-1e47b4de2f1a",
  "timestamp": "2025-07-03T11:25:00.000+09:00",
  "payload": {
    "state": "Auto"   // "Auto", "Paused", "Pausing"
  }
}
```

```json
// [MCS �� ACS] TSC ���� ������Ʈ ���ſ� ���� ����
{
  "command": "TscStateUpdateAck",
  "transactionId": "9f13f236-2c4b-42af-b94b-1e47b4de2f1a", 
  "timestamp": "2025-07-03T11:25:01.000+09:00",
  "result": "Success",               
  "message": "TSC state update received successfully.",
  "payload": {}
}
```

| state   | �ǹ�                                      |
| ------- | ----------------------------------------- |
| Auto    | Plan�� ���� �� �� �ִ� ����               |
| Paused  | Plan ������ �Ͻ� ���� �� ����             |
| Pausing | Paused ���� �õ� �� �Ϸ��ϱ� �� ��� ���� |

?? TscStateUpdate �޽����� �ʱ� ���� �� �ѹ�, ���ķδ� ���� ���� �ÿ� �����ϴ�.

---



### 6.21 ACS ��� ���� ���� (AcsCommStateUpdate)

```json
// [ACS �� MCS] ACS ��� ���� ���� ����
{
  "command": "AcsCommStateUpdate",
  "transactionId": "c7c8c9ae-3aa5-4f9e-bbfa-8140a59c94b6",
  "timestamp": "2025-08-07T11:10:00.000+09:00",
  "payload": {
    "isConnected": true    // true: �����, false: ���� ���� �Ǵ� ���� ��� ��
  }
}

```

```json
// [MCS �� ACS] AcsCommStateUpdate�� ���� ����
{
  "command": "AcsCommStateUpdateAck",
  "transactionId": "c7c8c9ae-3aa5-4f9e-bbfa-8140a59c94b6",
  "timestamp": "2025-08-07T11:10:00.150+09:00",
  "result": "Success",
  "message": "Comm state received.",
  "payload": {}
}

```

#### Abnormal Case 01. isConnected = false

- `isConnected`�� `false`�� ����� ���, �ش� ACS�� ����� �Ҿ����ϰų� ������ ���� ���·� ������
- �� ���¿����� **MCS�� �ش� ACS�� Plan�� �������� �ʽ��ϴ�**
- ������ �����Ǿ� `isConnected = true`�� ��ȯ�Ǳ� �������� Plan ���� �Ǵ� �������� ������



---





## 7. Ȯ�� �� �� ����

���� ���� �� �� �޽��� Ÿ��, �ó�����, �� ������ ������ �߰� ���� �ʿ�

---



## 8.����

* ��� CP, TP, MP �� Port ������ �� ����(����Ŀ, �κ� ��) ���� �� �̺�Ʈ �޽������� ����

* ������ ������ ���� �䱸, ������Ʈ ��Ȳ � ���� ����� �� ����



### 8.1 ��� �� �����ڷ� 

**<span style="color: red;">* ���� ������ �ٸ� �� ����.</span>**

- **����Ŀ(ST)**: 1��
  - **ī��Ʈ ��Ʈ(CP)**: 6��

- **�����:** 1�� 
  - **ī��Ʈ ��Ʈ(CP)**: 6��
    - **Ʈ���� ��Ʈ(TP)**: 4��
  - **��Ʈ(SET):** 12��
    - **�޸� ��Ʈ(MP)**: 32��
- **�����κ�:** 1�� 
  - **ī��Ʈ ��Ʈ(CP)**: 1��
- **�۾��κ�:** 1�� 
  - **Ʈ���� ��Ʈ(TP)**: 3��
    - **�޸� ��Ʈ(MP)**: 25��



| No   | ����                     | FullPath            | ���                                 |
| ---- | ------------------------ | ------------------- | ------------------------------------ |
| 1    | ����Ŀ-ī��Ʈ��Ʈ        | ST01.CP01           | ����Ŀ1-ī��Ʈ��Ʈ1                  |
| 2    | ����Ŀ-ī��Ʈ��Ʈ        | ST01.CP02           | ����Ŀ1-ī��Ʈ��Ʈ2                  |
| 3    | ����Ŀ-ī��Ʈ��Ʈ        | ST01.CP03           | ����Ŀ1-ī��Ʈ��Ʈ3                  |
| 4    | ����Ŀ-ī��Ʈ��Ʈ        | ST01.CP04           | ����Ŀ1-ī��Ʈ��Ʈ4                  |
| 5    | ����Ŀ-ī��Ʈ��Ʈ        | ST01.CP05           | ����Ŀ1-ī��Ʈ��Ʈ5                  |
| 6    | ����Ŀ-ī��Ʈ��Ʈ        | ST01.CP06           | ����Ŀ1-ī��Ʈ��Ʈ6                  |
| 7    | �����-ī��Ʈ��Ʈ      | A01.CP01            | �����1-ī��Ʈ��Ʈ1                |
| 8    | �����-Ʈ������Ʈ      | A01.CP01.TP01       | �����1-ī��Ʈ��Ʈ1-Ʈ������Ʈ1    |
| 9    | �����-Ʈ������Ʈ      | A01.CP01.TP02       | �����1-ī��Ʈ��Ʈ1-Ʈ������Ʈ2    |
| 10   | �����-Ʈ������Ʈ      | A01.CP01.TP03       | �����1-ī��Ʈ��Ʈ1-Ʈ������Ʈ3    |
| 11   | �����-Ʈ������Ʈ      | A01.CP01.TP04       | �����1-ī��Ʈ��Ʈ1-Ʈ������Ʈ4    |
| 12   | �����-ī��Ʈ��Ʈ      | A01.CP02            | �����1-ī��Ʈ��Ʈ2                |
| 13   | �����-Ʈ������Ʈ      | A01.CP02.TP01       | �����1-ī��Ʈ��Ʈ2-Ʈ������Ʈ1    |
| 14   | �����-Ʈ������Ʈ      | A01.CP02.TP02       | �����1-ī��Ʈ��Ʈ2-Ʈ������Ʈ2    |
| 15   | �����-Ʈ������Ʈ      | A01.CP02.TP03       | �����1-ī��Ʈ��Ʈ2-Ʈ������Ʈ3    |
| 16   | �����-Ʈ������Ʈ      | A01.CP02.TP04       | �����1-ī��Ʈ��Ʈ2-Ʈ������Ʈ4    |
| 17   | �����-ī��Ʈ��Ʈ      | A01.CP03            | �����1-ī��Ʈ��Ʈ3                |
| 18   | �����-Ʈ������Ʈ      | A01.CP03.TP01       | �����1-ī��Ʈ��Ʈ3-Ʈ������Ʈ1    |
| 19   | �����-Ʈ������Ʈ      | A01.CP03.TP02       | �����1-ī��Ʈ��Ʈ3-Ʈ������Ʈ2    |
| 20   | �����-Ʈ������Ʈ      | A01.CP03.TP03       | �����1-ī��Ʈ��Ʈ3-Ʈ������Ʈ3    |
| 21   | �����-Ʈ������Ʈ      | A01.CP03.TP04       | �����1-ī��Ʈ��Ʈ3-Ʈ������Ʈ4    |
| 22   | �����-ī��Ʈ��Ʈ      | A01.CP04            | �����1-ī��Ʈ��Ʈ4                |
| 23   | �����-Ʈ������Ʈ      | A01.CP04.TP01       | �����1-ī��Ʈ��Ʈ4-Ʈ������Ʈ1    |
| 24   | �����-Ʈ������Ʈ      | A01.CP04.TP02       | �����1-ī��Ʈ��Ʈ4-Ʈ������Ʈ2    |
| 25   | �����-Ʈ������Ʈ      | A01.CP04.TP03       | �����1-ī��Ʈ��Ʈ4-Ʈ������Ʈ3    |
| 26   | �����-Ʈ������Ʈ      | A01.CP04.TP04       | �����1-ī��Ʈ��Ʈ4-Ʈ������Ʈ4    |
| 27   | �����-ī��Ʈ��Ʈ      | A01.CP05            | �����1-ī��Ʈ��Ʈ5                |
| 28   | �����-Ʈ������Ʈ      | A01.CP05.TP01       | �����1-ī��Ʈ��Ʈ5-Ʈ������Ʈ1    |
| 29   | �����-Ʈ������Ʈ      | A01.CP05.TP02       | �����1-ī��Ʈ��Ʈ5-Ʈ������Ʈ2    |
| 30   | �����-Ʈ������Ʈ      | A01.CP05.TP03       | �����1-ī��Ʈ��Ʈ5-Ʈ������Ʈ3    |
| 31   | �����-Ʈ������Ʈ      | A01.CP05.TP04       | �����1-ī��Ʈ��Ʈ5-Ʈ������Ʈ4    |
| 32   | �����-ī��Ʈ��Ʈ      | A01.CP06            | �����1-ī��Ʈ��Ʈ6                |
| 33   | �����-Ʈ������Ʈ      | A01.CP06.TP01       | �����1-ī��Ʈ��Ʈ6-Ʈ������Ʈ1    |
| 34   | �����-Ʈ������Ʈ      | A01.CP06.TP02       | �����1-ī��Ʈ��Ʈ6-Ʈ������Ʈ2    |
| 35   | �����-Ʈ������Ʈ      | A01.CP06.TP03       | �����1-ī��Ʈ��Ʈ6-Ʈ������Ʈ3    |
| 36   | �����-Ʈ������Ʈ      | A01.CP06.TP04       | �����1-ī��Ʈ��Ʈ6-Ʈ������Ʈ4    |
| 37   | �����-��Ʈ            | A01.SET01           | �����1-��Ʈ1                      |
| 38   | �����-��Ʈ-�޸���Ʈ | A01.SET01.MP01~MP32 | �����1-��Ʈ1-�޸���Ʈ1~32       |
| 39   | �����-��Ʈ            | A01.SET02           | �����1-��Ʈ2                      |
| 40   | �����-��Ʈ-�޸���Ʈ | A01.SET02.MP01~MP32 | �����1-��Ʈ2-�޸���Ʈ1~32       |
| 41   | �����-��Ʈ            | A01.SET03           | �����1-��Ʈ3                      |
| 42   | �����-��Ʈ-�޸���Ʈ | A01.SET03.MP01~MP32 | �����1-��Ʈ3-�޸���Ʈ1~32       |
| 43   | �����-��Ʈ            | A01.SET04           | �����1-��Ʈ4                      |
| 44   | �����-��Ʈ-�޸���Ʈ | A01.SET04.MP01~MP32 | �����1-��Ʈ4-�޸���Ʈ1~32       |
| 45   | �����-��Ʈ            | A01.SET05           | �����1-��Ʈ5                      |
| 46   | �����-��Ʈ-�޸���Ʈ | A01.SET05.MP01~MP32 | �����1-��Ʈ5-�޸���Ʈ1~32       |
| 47   | �����-��Ʈ            | A01.SET06           | �����1-��Ʈ6                      |
| 48   | �����-��Ʈ-�޸���Ʈ | A01.SET06.MP01~MP32 | �����1-��Ʈ6-�޸���Ʈ1~32       |
| 49   | �����-��Ʈ            | A01.SET07           | �����1-��Ʈ7                      |
| 50   | �����-��Ʈ-�޸���Ʈ | A01.SET07.MP01~MP32 | �����1-��Ʈ7-�޸���Ʈ1~32       |
| 51   | �����-��Ʈ            | A01.SET08           | �����1-��Ʈ8                      |
| 52   | �����-��Ʈ-�޸���Ʈ | A01.SET08.MP01~MP32 | �����1-��Ʈ8-�޸���Ʈ1~32       |
| 53   | �����-��Ʈ            | A01.SET09           | �����1-��Ʈ9                      |
| 54   | �����-��Ʈ-�޸���Ʈ | A01.SET09.MP01~MP32 | �����1-��Ʈ9-�޸���Ʈ1~32       |
| 55   | �����-��Ʈ            | A01.SET10           | �����1-��Ʈ10                     |
| 56   | �����-��Ʈ-�޸���Ʈ | A01.SET10.MP01~MP32 | �����1-��Ʈ10-�޸���Ʈ1~32      |
| 57   | �����-��Ʈ            | A01.SET11           | �����1-��Ʈ11                     |
| 58   | �����-��Ʈ-�޸���Ʈ | A01.SET11.MP01~MP32 | �����1-��Ʈ11-�޸���Ʈ1~32      |
| 59   | �����-��Ʈ            | A01.SET12           | �����1-��Ʈ12                     |
| 60   | �����-��Ʈ-�޸���Ʈ | A01.SET12.MP01~MP32 | �����1-��Ʈ12-�޸���Ʈ1~32      |
| 61   | �����κ�-ī��Ʈ��Ʈ      | AMR.CP01            | �����κ�1-ī��Ʈ��Ʈ1                |
| 62   | �۾��κ�-Ʈ������Ʈ      | AMR.TP01            | �۾��κ�1-Ʈ������Ʈ1                |
| 63   | �۾��κ�-�޸���Ʈ      | AMR.TP01.MP01~MP25  | �۾��κ�1-Ʈ������Ʈ1-�޸���Ʈ1~25 |
| 64   | �۾��κ�-Ʈ������Ʈ      | AMR.TP02            | �۾��κ�1-Ʈ������Ʈ2                |
| 65   | �۾��κ�-�޸���Ʈ      | AMR.TP02.MP01~MP25  | �۾��κ�1-Ʈ������Ʈ2-�޸���Ʈ1~25 |
| 66   | �۾��κ�-Ʈ������Ʈ      | AMR.TP03            | �۾��κ�1-Ʈ������Ʈ3                |
| 67   | �۾��κ�-�޸���Ʈ      | AMR.TP03.MP01~MP25  | �۾��κ�1-Ʈ������Ʈ3-�޸���Ʈ1~25 |





## 9. ���� ����

- **����**: v1.4.1
- **�ۼ���**: 2025-08-11
- **�ۼ���**: ���̿�����Ʈ ����ȣ
- **���**:  �� ������ ���� ��û �Ǵ� ���� ���� �������� ������ ����� �� �ֽ��ϴ�.

---



### 9.1 ���� ����

* **v1.4.1**
  - **<span style="color: red;">6.10 �÷� ��Ȳ ��û (RequestAcsPlans)  </span>**
    - `status` ���� ���� `stepNo`, `jobId` ǥ�� ��Ģ �߰�
      - Pending�� ���, ù ��° Step�� �ش� Step�� ù ��° Job
      - InProgress�� ���, ���� ���� Step�� ���� ���� Job
      - Paused�� ���, ���� ���̴� Step�� �Ͻ� ������ Job
  - <span style="color: red;">**6.11 ACS �÷� �̷� ��ȸ (RequestAcsPlanHistory)**</span>
    - `status` ���� ���� `stepNo`, `jobId` ǥ�� ��Ģ �߰�
      - Failed�� ���, ���а� �߻��� *Step*�� �ش� *Job*
      - Completed�� ���, *stepNo*�� 0,  *jobId*�� `null`

- **v1.4.0**
  - �۾� (���/�ߴ�/�Ͻ�����/�簳) Ŀ�ǵ� �� ��� ���� Ŀ�ǵ� �߰�
  - Abnormal Case �߻� �� ���� ����
  - 6.21 ACS ��� ���� ���� (AcsCommStateUpdate) Ŀ�ǵ� �߰�

- **v1.3.0**
  - 6.20 TSC ���� ���� �޼��� �߰�
  
- **v1.2.1**
  - 6.16 �κ� ���� ������Ʈ Ŀ�ǵ� ���� (*robotType* �Ӽ� �߰�)
  - 6.19 ���͸� �����̼� ������Ʈ Ŀ�ǵ� ����

- **v1.2.0**
  - 5.3 �κ� ��ġ, ���͸� �����̼� ���� ���� ������ �ֱ������� �����ϴ� Ŀ�ǵ� �׸� �߰� (6.18, 6.19 Ŀ�ǵ�)
  - 6.16 �κ� ���� ���� ���� RobotStatusUpdate �޼��� ���� (*battery* �Ӽ� ����)
  - 6.16 �κ� ���� ���� ���� RobotStatusUpdate �޼��� (*carrierIds* ���� �߰� - TP�� ����ִ� ���)

- **v1.1.0**
  - MCS - ACS ��� Ŭ���̾�Ʈ�� �ϳ��� ���
  - 6.2 Registration �޼��� ���� (*robotType* ����)
  - 6.14 Job ���� ���� �޼��� �� *Status* �Ӽ� �� ��[*Retrying*] ���� ��� ����
  - 6.3~6.4 ExecutionPlan ���� ��� �޼��� ���� (*carrierIds* �߰�)
  - 6.15 ����/�˶� �̺�Ʈ ���� �޼��� �� �Ӽ� �߰� (���� ���� ���/ level)
  - 6.16 �κ� ���� ���� ���� RobotStatusUpdate �޼��� ���� (*ports* ����, *carrierIds* �߰�)
  - 6.17 ����/�˶� ��Ȳ ��û �޼��� �߰� (RequestAcsErrorList)
  - 6.10 �÷� ��Ȳ ��û �޼����� ACS ������ �����ϴ� �÷� ����Ʈ�� status �Ӽ��� (*Pending, InProgress, Paused*)�� ����

- **v1.0.1**
  - �ʱ� ���� (Registration) �޼��� ��  payload ����. (Ʈ���� / ī��Ʈ ���� �ۼ��� ����)

  - �÷� ���� ���� �޼��� �߰�.

  - RequestAcsStatus �޼��� �� payload ����.

  - StatusUpdate �޼��� ��Ī ���� (RobotStatusUpdate)

  - RobotStatusUpdate (Ʈ����/ī��Ʈ ���� )

  - RequestAcsStatus �޼��� ��Ī ���� (RequestAcsPlans)
  - ACS �÷� �̷� ��ȸ �޼��� �߰� 


