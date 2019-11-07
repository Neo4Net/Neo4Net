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
namespace Neo4Net.Kernel.impl.util.dbstructure
{
	using Neo4Net.Collections.Helpers;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.Api.schema.constraints.ConstraintDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.Api.schema.index.TestIndexDescriptorFactory;

	//
	// GENERATED FILE. DO NOT EDIT.
	//
	// This has been generated by:
	//
	//   Neo4Net.kernel.impl.util.dbstructure.DbStructureTool
	//   Neo4Net.kernel.impl.util.dbstructure.CineastsDbStructure [<output source root>] <db-dir>
	//
	// (using Neo4Net.kernel.impl.util.dbstructure.InvocationTracer)
	//

	public sealed class CineastsDbStructure : Visitable<DbStructureVisitor>
	{
		 public static readonly CineastsDbStructure Instance = new CineastsDbStructure( "Instance", InnerEnum.Instance );

		 private static readonly IList<CineastsDbStructure> valueList = new List<CineastsDbStructure>();

		 static CineastsDbStructure()
		 {
			 valueList.Add( Instance );
		 }

		 public enum InnerEnum
		 {
			 Instance
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private CineastsDbStructure( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( DbStructureVisitor visitor )
		 {
			  visitor.VisitLabel( 0, "Movie" );
			  visitor.VisitLabel( 1, "Person" );
			  visitor.VisitLabel( 2, "User" );
			  visitor.VisitLabel( 3, "Actor" );
			  visitor.VisitLabel( 4, "Director" );
			  visitor.VisitPropertyKey( 0, "startTime" );
			  visitor.VisitPropertyKey( 1, "__type__" );
			  visitor.VisitPropertyKey( 2, "password" );
			  visitor.VisitPropertyKey( 3, "login" );
			  visitor.VisitPropertyKey( 4, "roles" );
			  visitor.VisitPropertyKey( 5, "name" );
			  visitor.VisitPropertyKey( 6, "description" );
			  visitor.VisitPropertyKey( 7, "id" );
			  visitor.VisitPropertyKey( 8, "releaseDate" );
			  visitor.VisitPropertyKey( 9, "title" );
			  visitor.VisitPropertyKey( 10, "tagline" );
			  visitor.VisitPropertyKey( 11, "language" );
			  visitor.VisitPropertyKey( 12, "imageUrl" );
			  visitor.VisitPropertyKey( 13, "lastModified" );
			  visitor.VisitPropertyKey( 14, "genre" );
			  visitor.VisitPropertyKey( 15, "studio" );
			  visitor.VisitPropertyKey( 17, "imdbId" );
			  visitor.VisitPropertyKey( 16, "trailer" );
			  visitor.VisitPropertyKey( 19, "homepage" );
			  visitor.VisitPropertyKey( 18, "version" );
			  visitor.VisitPropertyKey( 21, "profileImageUrl" );
			  visitor.VisitPropertyKey( 20, "runtime" );
			  visitor.VisitPropertyKey( 23, "birthday" );
			  visitor.VisitPropertyKey( 22, "biography" );
			  visitor.VisitPropertyKey( 25, "stars" );
			  visitor.VisitPropertyKey( 24, "birthplace" );
			  visitor.VisitPropertyKey( 26, "comment" );
			  visitor.VisitRelationshipType( 0, "FRIEND" );
			  visitor.VisitRelationshipType( 1, "DIRECTED" );
			  visitor.VisitRelationshipType( 2, "ACTS_IN" );
			  visitor.VisitRelationshipType( 3, "RATED" );
			  visitor.VisitRelationshipType( 4, "ROOT" );
			  visitor.VisitIndex( TestIndexDescriptorFactory.forLabel( 0, 9 ), ":Movie(title)", 1.0d, 12462L );
			  visitor.VisitIndex( TestIndexDescriptorFactory.forLabel( 1, 5 ), ":Person(name)", 1.0d, 49845L );
			  visitor.VisitIndex( TestIndexDescriptorFactory.forLabel( 3, 5 ), ":Actor(name)", 1.0d, 44689L );
			  visitor.VisitIndex( TestIndexDescriptorFactory.forLabel( 4, 5 ), ":Director(name)", 1.0d, 6010L );
			  visitor.VisitIndex( TestIndexDescriptorFactory.uniqueForLabel( 2, 3 ), ":User(login)", 1.0d, 45L );
			  visitor.VisitUniqueConstraint( ConstraintDescriptorFactory.uniqueForLabel( 2, 3 ), "CONSTRAINT ON ( " + "user:User ) ASSERT user.login IS UNIQUE" );
			  visitor.VisitAllNodesCount( 63042L );
			  visitor.VisitNodeCount( 0, "Movie", 12862L );
			  visitor.VisitNodeCount( 1, "Person", 50179L );
			  visitor.VisitNodeCount( 2, "User", 45L );
			  visitor.VisitNodeCount( 3, "Actor", 44943L );
			  visitor.VisitNodeCount( 4, "Director", 6037L );
			  visitor.VisitRelCount( -1, -1, -1, "MATCH ()-[]->() RETURN count(*)", 106651L );
			  visitor.VisitRelCount( 0, -1, -1, "MATCH (:Movie)-[]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, -1, 0, "MATCH ()-[]->(:Movie) RETURN count(*)", 106645L );
			  visitor.VisitRelCount( 1, -1, -1, "MATCH (:Person)-[]->() RETURN count(*)", 106651L );
			  visitor.VisitRelCount( -1, -1, 1, "MATCH ()-[]->(:Person) RETURN count(*)", 6L );
			  visitor.VisitRelCount( 2, -1, -1, "MATCH (:User)-[]->() RETURN count(*)", 36L );
			  visitor.VisitRelCount( -1, -1, 2, "MATCH ()-[]->(:User) RETURN count(*)", 6L );
			  visitor.VisitRelCount( 3, -1, -1, "MATCH (:Actor)-[]->() RETURN count(*)", 97151L );
			  visitor.VisitRelCount( -1, -1, 3, "MATCH ()-[]->(:Actor) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 4, -1, -1, "MATCH (:Director)-[]->() RETURN count(*)", 16268L );
			  visitor.VisitRelCount( -1, -1, 4, "MATCH ()-[]->(:Director) RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 0, -1, "MATCH ()-[:FRIEND]->() RETURN count(*)", 6L );
			  visitor.VisitRelCount( 0, 0, -1, "MATCH (:Movie)-[:FRIEND]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 0, 0, "MATCH ()-[:FRIEND]->(:Movie) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 1, 0, -1, "MATCH (:Person)-[:FRIEND]->() RETURN count(*)", 6L );
			  visitor.VisitRelCount( -1, 0, 1, "MATCH ()-[:FRIEND]->(:Person) RETURN count(*)", 6L );
			  visitor.VisitRelCount( 2, 0, -1, "MATCH (:User)-[:FRIEND]->() RETURN count(*)", 6L );
			  visitor.VisitRelCount( -1, 0, 2, "MATCH ()-[:FRIEND]->(:User) RETURN count(*)", 6L );
			  visitor.VisitRelCount( 3, 0, -1, "MATCH (:Actor)-[:FRIEND]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 0, 3, "MATCH ()-[:FRIEND]->(:Actor) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 4, 0, -1, "MATCH (:Director)-[:FRIEND]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 0, 4, "MATCH ()-[:FRIEND]->(:Director) RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 1, -1, "MATCH ()-[:DIRECTED]->() RETURN count(*)", 11915L );
			  visitor.VisitRelCount( 0, 1, -1, "MATCH (:Movie)-[:DIRECTED]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 1, 0, "MATCH ()-[:DIRECTED]->(:Movie) RETURN count(*)", 11915L );
			  visitor.VisitRelCount( 1, 1, -1, "MATCH (:Person)-[:DIRECTED]->() RETURN count(*)", 11915L );
			  visitor.VisitRelCount( -1, 1, 1, "MATCH ()-[:DIRECTED]->(:Person) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 2, 1, -1, "MATCH (:User)-[:DIRECTED]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 1, 2, "MATCH ()-[:DIRECTED]->(:User) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 3, 1, -1, "MATCH (:Actor)-[:DIRECTED]->() RETURN count(*)", 2451L );
			  visitor.VisitRelCount( -1, 1, 3, "MATCH ()-[:DIRECTED]->(:Actor) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 4, 1, -1, "MATCH (:Director)-[:DIRECTED]->() RETURN count(*)", 11915L );
			  visitor.VisitRelCount( -1, 1, 4, "MATCH ()-[:DIRECTED]->(:Director) RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 2, -1, "MATCH ()-[:ACTS_IN]->() RETURN count(*)", 94700L );
			  visitor.VisitRelCount( 0, 2, -1, "MATCH (:Movie)-[:ACTS_IN]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 2, 0, "MATCH ()-[:ACTS_IN]->(:Movie) RETURN count(*)", 94700L );
			  visitor.VisitRelCount( 1, 2, -1, "MATCH (:Person)-[:ACTS_IN]->() RETURN count(*)", 94700L );
			  visitor.VisitRelCount( -1, 2, 1, "MATCH ()-[:ACTS_IN]->(:Person) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 2, 2, -1, "MATCH (:User)-[:ACTS_IN]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 2, 2, "MATCH ()-[:ACTS_IN]->(:User) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 3, 2, -1, "MATCH (:Actor)-[:ACTS_IN]->() RETURN count(*)", 94700L );
			  visitor.VisitRelCount( -1, 2, 3, "MATCH ()-[:ACTS_IN]->(:Actor) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 4, 2, -1, "MATCH (:Director)-[:ACTS_IN]->() RETURN count(*)", 4353L );
			  visitor.VisitRelCount( -1, 2, 4, "MATCH ()-[:ACTS_IN]->(:Director) RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 3, -1, "MATCH ()-[:RATED]->() RETURN count(*)", 30L );
			  visitor.VisitRelCount( 0, 3, -1, "MATCH (:Movie)-[:RATED]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 3, 0, "MATCH ()-[:RATED]->(:Movie) RETURN count(*)", 30L );
			  visitor.VisitRelCount( 1, 3, -1, "MATCH (:Person)-[:RATED]->() RETURN count(*)", 30L );
			  visitor.VisitRelCount( -1, 3, 1, "MATCH ()-[:RATED]->(:Person) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 2, 3, -1, "MATCH (:User)-[:RATED]->() RETURN count(*)", 30L );
			  visitor.VisitRelCount( -1, 3, 2, "MATCH ()-[:RATED]->(:User) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 3, 3, -1, "MATCH (:Actor)-[:RATED]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 3, 3, "MATCH ()-[:RATED]->(:Actor) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 4, 3, -1, "MATCH (:Director)-[:RATED]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 3, 4, "MATCH ()-[:RATED]->(:Director) RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 4, -1, "MATCH ()-[:ROOT]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( 0, 4, -1, "MATCH (:Movie)-[:ROOT]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 4, 0, "MATCH ()-[:ROOT]->(:Movie) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 1, 4, -1, "MATCH (:Person)-[:ROOT]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 4, 1, "MATCH ()-[:ROOT]->(:Person) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 2, 4, -1, "MATCH (:User)-[:ROOT]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 4, 2, "MATCH ()-[:ROOT]->(:User) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 3, 4, -1, "MATCH (:Actor)-[:ROOT]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 4, 3, "MATCH ()-[:ROOT]->(:Actor) RETURN count(*)", 0L );
			  visitor.VisitRelCount( 4, 4, -1, "MATCH (:Director)-[:ROOT]->() RETURN count(*)", 0L );
			  visitor.VisitRelCount( -1, 4, 4, "MATCH ()-[:ROOT]->(:Director) RETURN count(*)", 0L );
		 }

		public static IList<CineastsDbStructure> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static CineastsDbStructure ValueOf( string name )
		{
			foreach ( CineastsDbStructure enumInstance in CineastsDbStructure.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

	/* END OF GENERATED CONTENT */

}