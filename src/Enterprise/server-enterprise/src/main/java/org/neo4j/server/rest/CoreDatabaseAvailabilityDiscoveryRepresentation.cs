﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Server.rest
{
	using MappingRepresentation = Neo4Net.Server.rest.repr.MappingRepresentation;
	using MappingSerializer = Neo4Net.Server.rest.repr.MappingSerializer;

	public class CoreDatabaseAvailabilityDiscoveryRepresentation : MappingRepresentation
	{
		 private const string WRITABLE_KEY = "writable";
		 private const string DISCOVERY_REPRESENTATION_TYPE = "discovery";

		 private readonly string _basePath;
		 private readonly string _isWritableUri;

		 public CoreDatabaseAvailabilityDiscoveryRepresentation( string basePath, string isWritableUri ) : base( DISCOVERY_REPRESENTATION_TYPE )
		 {
			  this._basePath = basePath;
			  this._isWritableUri = isWritableUri;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  serializer.PutRelativeUri( WRITABLE_KEY, _basePath + _isWritableUri );
		 }
	}

}