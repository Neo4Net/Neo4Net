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
namespace Org.Neo4j.Kernel.impl.util
{

	using Resource = Org.Neo4j.Graphdb.Resource;
	using IOUtils = Org.Neo4j.Io.IOUtils;

	public sealed class MultiResource : Resource
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Collection<? extends org.neo4j.graphdb.Resource> resources;
		 private readonly ICollection<Resource> _resources;

		 public MultiResource<T1>( ICollection<T1> resources ) where T1 : Org.Neo4j.Graphdb.Resource
		 {
			  this._resources = resources;
		 }

		 public override void Close()
		 {
			  try
			  {
					IOUtils.closeAll( _resources );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}