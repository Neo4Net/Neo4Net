﻿/*
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
namespace Org.Neo4j.Server.rest.repr
{
	using Version = Org.Neo4j.Kernel.@internal.Version;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_BATCH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_CYPHER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_EXTENSIONS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_LABELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_NODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_NODE_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_RELATIONSHIPS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_RELATIONSHIP_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_RELATIONSHIP_TYPES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_SCHEMA_CONSTRAINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_SCHEMA_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_TRANSACTION;

	public class DatabaseRepresentation : MappingRepresentation, ExtensibleRepresentation
	{

		 public DatabaseRepresentation() : base(RepresentationType.Graphdb)
		 {
		 }

		 public virtual string Identity
		 {
			 get
			 {
				  // This is in fact correct - there is only one graphdb - hence no id
				  return null;
			 }
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  serializer.PutRelativeUri( "node", PATH_NODES );
			  serializer.PutRelativeUri( "relationship", PATH_RELATIONSHIPS );
			  serializer.PutRelativeUri( "node_index", PATH_NODE_INDEX );
			  serializer.PutRelativeUri( "relationship_index", PATH_RELATIONSHIP_INDEX );
			  serializer.PutRelativeUri( "extensions_info", PATH_EXTENSIONS );
			  serializer.PutRelativeUri( "relationship_types", PATH_RELATIONSHIP_TYPES );
			  serializer.PutRelativeUri( "batch", PATH_BATCH );
			  serializer.PutRelativeUri( "cypher", PATH_CYPHER );
			  serializer.PutRelativeUri( "indexes", PATH_SCHEMA_INDEX );
			  serializer.PutRelativeUri( "constraints", PATH_SCHEMA_CONSTRAINT );
			  serializer.PutRelativeUri( "transaction", PATH_TRANSACTION );
			  serializer.PutRelativeUri( "node_labels", PATH_LABELS );
			  serializer.PutString( "neo4j_version", Version.Neo4jVersion );
		 }
	}

}