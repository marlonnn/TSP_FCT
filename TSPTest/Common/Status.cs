using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Common
{
    [Serializable]
    public enum TestStatus
    {
        THRESHOLD,
        HANDS_OK,
        HANDS_NOTOK,
        UNEXPECTED_FINNISH,
        RUNNING,
        EXPECTED_FINNISH,
    }
}
