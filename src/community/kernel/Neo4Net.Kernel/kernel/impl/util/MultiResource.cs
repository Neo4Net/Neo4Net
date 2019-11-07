using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.util
{

	using Resource = Neo4Net.GraphDb.Resource;
	using IOUtils = Neo4Net.Io.IOUtils;

	public sealed class MultiResource : Resource
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Collection<? extends Neo4Net.graphdb.Resource> resources;
		 private readonly ICollection<Resource> _resources;

		 public MultiResource<T1>( ICollection<T1> resources ) where T1 : Neo4Net.GraphDb.Resource
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