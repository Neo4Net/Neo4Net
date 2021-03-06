﻿using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Helpers
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asArray;

	[Obsolete]
	public class RunCarefully
	{
		 private readonly ThreadStart[] _operations;

		 [Obsolete]
		 public RunCarefully( params ThreadStart[] operations )
		 {
			  this._operations = operations;
		 }

		 [Obsolete]
		 public RunCarefully( IEnumerable<ThreadStart> operations ) : this( asArray( typeof( ThreadStart ), operations ) )
		 {
		 }

		 [Obsolete]
		 public virtual void Run()
		 {
			  IList<Exception> errors = new List<Exception>();

			  foreach ( ThreadStart o in _operations )
			  {
					try
					{
						 o.run();
					}
					catch ( Exception e )
					{
						 errors.Add( e );
					}
			  }

			  if ( errors.Count > 0 )
			  {
					Exception exception = new Exception();
					errors.ForEach( exception.addSuppressed );
					throw exception;
			  }
		 }
	}

}