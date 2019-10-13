﻿using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.management.repr
{
	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using MappingSerializer = Neo4Net.Server.rest.repr.MappingSerializer;

	public class ConsoleServiceRepresentation : ServiceDefinitionRepresentation
	{

		 private IEnumerable<string> _engines;

		 public ConsoleServiceRepresentation( string basePath, IEnumerable<string> engines ) : base( basePath )
		 {
			  ResourceUri( "exec", "" );
			  this._engines = engines;
		 }

		 public override void Serialize( MappingSerializer serializer )
		 {
			  base.Serialize( serializer );
			  serializer.PutList( "engines", ListRepresentation.@string( _engines ) );
		 }

	}

}