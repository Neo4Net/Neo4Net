﻿using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Harness.@internal
{

	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;

	public class HarnessRegisteredProcs
	{
		 private readonly IList<Type> _procs = new LinkedList<Type>();
		 private readonly IList<Type> _functions = new LinkedList<Type>();
		 private readonly IList<Type> _aggregationFunctions = new LinkedList<Type>();

		 public virtual void AddProcedure( Type procedureClass )
		 {
			  this._procs.Add( procedureClass );
		 }

		 public virtual void AddFunction( Type functionClass )
		 {
			  this._functions.Add( functionClass );
		 }

		 public virtual void AddAggregationFunction( Type functionClass )
		 {
			  this._aggregationFunctions.Add( functionClass );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") public void applyTo(org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ApplyTo( Procedures procedures )
		 {
			  foreach ( Type cls in _procs )
			  {
					procedures.RegisterProcedure( cls );
			  }

			  foreach ( Type cls in _functions )
			  {
					procedures.RegisterFunction( cls );
			  }

			  foreach ( Type cls in _aggregationFunctions )
			  {
					procedures.RegisterAggregationFunction( cls );
			  }
		 }
	}

}