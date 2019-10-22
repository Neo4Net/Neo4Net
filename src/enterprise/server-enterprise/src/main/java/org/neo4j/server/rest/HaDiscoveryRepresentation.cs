/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.rest
{
	using MappingRepresentation = Neo4Net.Server.rest.repr.MappingRepresentation;
	using MappingSerializer = Neo4Net.Server.rest.repr.MappingSerializer;

	public class HaDiscoveryRepresentation : MappingRepresentation
	{
		 private const string MASTER_KEY = "master";
		 private const string SLAVE_KEY = "slave";
		 private const string DISCOVERY_REPRESENTATION_TYPE = "discovery";

		 private readonly string _basePath;
		 private readonly string _isMasterUri;
		 private readonly string _isSlaveUri;

		 public HaDiscoveryRepresentation( string basePath, string isMasterUri, string isSlaveUri ) : base( DISCOVERY_REPRESENTATION_TYPE )
		 {
			  this._basePath = basePath;
			  this._isMasterUri = isMasterUri;
			  this._isSlaveUri = isSlaveUri;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  serializer.PutRelativeUri( MASTER_KEY, _basePath + _isMasterUri );
			  serializer.PutRelativeUri( SLAVE_KEY, _basePath + _isSlaveUri );
		 }
	}

}