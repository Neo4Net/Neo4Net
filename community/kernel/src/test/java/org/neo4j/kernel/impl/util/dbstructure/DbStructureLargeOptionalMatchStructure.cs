﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.util.dbstructure
{
	using Org.Neo4j.Helpers.Collection;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;

	//
	// GENERATED FILE. DO NOT EDIT.
	//
	// This has been generated by:
	//
	//   org.neo4j.kernel.impl.util.dbstructure.DbStructureTool
	//   org.neo4j.kernel.impl.util.dbstructure.DbStructureLargeOptionalMatchStructure [<output source root>] <db-dir>
	//
	// (using org.neo4j.kernel.impl.util.dbstructure.InvocationTracer)
	//

	public sealed class DbStructureLargeOptionalMatchStructure : Visitable<DbStructureVisitor>
	{
		 public static readonly DbStructureLargeOptionalMatchStructure Instance = new DbStructureLargeOptionalMatchStructure( "Instance", InnerEnum.Instance );

		 private static readonly IList<DbStructureLargeOptionalMatchStructure> valueList = new List<DbStructureLargeOptionalMatchStructure>();

		 static DbStructureLargeOptionalMatchStructure()
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

		 private DbStructureLargeOptionalMatchStructure( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( DbStructureVisitor visitor )
		 {
			  visitor.VisitLabel( 2, "Label20" );
			  visitor.VisitLabel( 3, "Label4" );
			  visitor.VisitLabel( 4, "Label22" );
			  visitor.VisitLabel( 5, "Label6" );
			  visitor.VisitLabel( 7, "Label1" );
			  visitor.VisitLabel( 8, "Label12" );
			  visitor.VisitLabel( 9, "Label14" );
			  visitor.VisitLabel( 10, "Label26" );
			  visitor.VisitLabel( 11, "Label10" );
			  visitor.VisitLabel( 12, "Label24" );
			  visitor.VisitLabel( 13, "Label8" );
			  visitor.VisitLabel( 14, "Label18" );
			  visitor.VisitLabel( 15, "Label19" );
			  visitor.VisitLabel( 16, "Label3" );
			  visitor.VisitLabel( 17, "Label16" );
			  visitor.VisitLabel( 18, "Label15" );
			  visitor.VisitLabel( 19, "Label21" );
			  visitor.VisitLabel( 20, "Label5" );
			  visitor.VisitLabel( 22, "Label2" );
			  visitor.VisitLabel( 23, "Label11" );
			  visitor.VisitLabel( 24, "Label13" );
			  visitor.VisitLabel( 25, "Label17" );
			  visitor.VisitLabel( 26, "Label25" );
			  visitor.VisitLabel( 27, "Label9" );
			  visitor.VisitLabel( 28, "Label23" );
			  visitor.VisitLabel( 29, "Label7" );
			  visitor.VisitLabel( 31, "Label27" );
			  visitor.VisitPropertyKey( 0, "id" );
			  visitor.VisitPropertyKey( 27, "deleted" );
			  visitor.VisitRelationshipType( 1, "REL1" );
			  visitor.VisitRelationshipType( 2, "REL4" );
			  visitor.VisitRelationshipType( 4, "REL2" );
			  visitor.VisitRelationshipType( 5, "REL5" );
			  visitor.VisitRelationshipType( 6, "REL8" );
			  visitor.VisitRelationshipType( 9, "REL6" );
			  visitor.VisitIndex( TestIndexDescriptorFactory.forLabel( 22, 0 ), ":Label2(id)", 0.3641877706337751d, 304838L );
			  visitor.VisitAllNodesCount( 2668827L );
			  visitor.VisitNodeCount( 2, "Label20", 3L );
			  visitor.VisitNodeCount( 3, "Label4", 0L );
			  visitor.VisitNodeCount( 4, "Label22", 0L );
			  visitor.VisitNodeCount( 5, "Label6", 0L );
			  visitor.VisitNodeCount( 7, "Label1", 111110L );
			  visitor.VisitNodeCount( 8, "Label12", 111112L );
			  visitor.VisitNodeCount( 9, "Label14", 99917L );
			  visitor.VisitNodeCount( 10, "Label26", 3L );
			  visitor.VisitNodeCount( 11, "Label10", 111150L );
			  visitor.VisitNodeCount( 12, "Label24", 0L );
			  visitor.VisitNodeCount( 13, "Label8", 0L );
			  visitor.VisitNodeCount( 14, "Label18", 111112L );
			  visitor.VisitNodeCount( 15, "Label19", 3L );
			  visitor.VisitNodeCount( 16, "Label3", 0L );
			  visitor.VisitNodeCount( 17, "Label16", 0L );
			  visitor.VisitNodeCount( 18, "Label15", 0L );
			  visitor.VisitNodeCount( 19, "Label21", 0L );
			  visitor.VisitNodeCount( 20, "Label5", 0L );
			  visitor.VisitNodeCount( 22, "Label2", 310059L );
			  visitor.VisitNodeCount( 23, "Label11", 179015L );
			  visitor.VisitNodeCount( 24, "Label13", 99917L );
			  visitor.VisitNodeCount( 25, "Label17", 179021L );
			  visitor.VisitNodeCount( 26, "Label25", 3L );
			  visitor.VisitNodeCount( 27, "Label9", 111150L );
			  visitor.VisitNodeCount( 28, "Label23", 0L );
			  visitor.VisitNodeCount( 29, "Label7", 0L );
			  visitor.VisitNodeCount( 31, "Label27", 1567352L );
			  visitor.VisitRelCount( -1, -1, -1, "MATCH ()-[]->() RETURN count(*)", 4944492L );

		 }

		public static IList<DbStructureLargeOptionalMatchStructure> values()
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

		public static DbStructureLargeOptionalMatchStructure valueOf( string name )
		{
			foreach ( DbStructureLargeOptionalMatchStructure enumInstance in DbStructureLargeOptionalMatchStructure.valueList )
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