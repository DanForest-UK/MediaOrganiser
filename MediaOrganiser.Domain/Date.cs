﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganiser.Domain;

/// <summary>
/// File date type
/// </summary>
public readonly record struct Date(DateTime Value);
