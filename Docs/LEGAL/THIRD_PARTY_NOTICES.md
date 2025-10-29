# Third-Party Notices (Refreshed for UGS)

This project uses Unity Gaming Services (UGS) and additional third-party libraries. The following notices provide license information and references. Distribute this file together with the product.

---

## 1. Unity Gaming Services (UGS)
UGS packages are provided by Unity. Refer to Unity terms and the individual package licenses for details.

- Unity Services Core, Authentication, Cloud Save, Cloud Code, Economy
  - Terms: https://unity.com/legal/terms-of-service
  - Package licenses and notices are included within each package (Package Manager > package > View documentation) and under `Library/PackageCache/`.

Note: UGS packages may depend on additional libraries. Their notices are included within each package folder.

---

## 2. Newtonsoft.Json (Unity Package)
- Package: `com.unity.nuget.newtonsoft-json`
- Upstream: https://www.newtonsoft.com/json
- License: MIT License
- Notice: Unity-distributed package of Newtonsoft.Json. The MIT License text is reproduced below.

```
The MIT License (MIT)

Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```

---

## 3. Other Third-Party Components (Present in Project)
The repository may include additional third-party packages in `Assets/Packages/` or via the Package Manager. Please consult each package's license included in its distribution.

Examples detected in this repository (subject to change):
- Microsoft IdentityModel libraries (Jwt/Abstractions/Tokens) and System.* packages distributed via NuGet-for-Unity
  - See each package folder under `Assets/Packages/` for license files and notices.

If you distribute builds that include these packages, you must comply with their respective licenses and include their notices.

---

## 4. How to Update This Document
- When adding or removing dependencies, update this file accordingly.
- For Unity packages, reference the package documentation and license from the Package Manager.
- For Assets/Packages, include the license and upstream URL for each redistributed component.

---

## 5. Historical Note
This document was refreshed to remove GS2-related notices and align with a UGS-based architecture.
