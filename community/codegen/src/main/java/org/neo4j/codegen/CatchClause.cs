﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Codegen
{

	public class CatchClause
	{
		 private readonly Parameter _exception;
		 private readonly IList<System.Action<MethodEmitter>> _actions;

		 public CatchClause( Parameter exception, IList<System.Action<MethodEmitter>> actions )
		 {
			  this._exception = exception;
			  this._actions = actions;
		 }

		 public virtual Parameter Exception()
		 {
			  return _exception;
		 }

		 public virtual IList<System.Action<MethodEmitter>> Actions()
		 {
			  return _actions;
		 }

	}

}