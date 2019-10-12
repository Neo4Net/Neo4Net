/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Server.rest.causalclustering
{
	using MappingRepresentation = Org.Neo4j.Server.rest.repr.MappingRepresentation;
	using MappingSerializer = Org.Neo4j.Server.rest.repr.MappingSerializer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.causalclustering.CausalClusteringService.AVAILABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.causalclustering.CausalClusteringService.READ_ONLY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.causalclustering.CausalClusteringService.DESCRIPTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.causalclustering.CausalClusteringService.WRITABLE;

	public class CausalClusteringDiscovery : MappingRepresentation
	{
		 private const string DISCOVERY_REPRESENTATION_TYPE = "discovery";

		 private readonly string _basePath;

		 internal CausalClusteringDiscovery( string basePath ) : base( DISCOVERY_REPRESENTATION_TYPE )
		 {
			  this._basePath = basePath;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  serializer.PutRelativeUri( AVAILABLE, _basePath + "/" + AVAILABLE );
			  serializer.PutRelativeUri( READ_ONLY, _basePath + "/" + READ_ONLY );
			  serializer.PutRelativeUri( WRITABLE, _basePath + "/" + WRITABLE );
			  serializer.PutRelativeUri( DESCRIPTION, _basePath + "/" + DESCRIPTION );
		 }
	}

}