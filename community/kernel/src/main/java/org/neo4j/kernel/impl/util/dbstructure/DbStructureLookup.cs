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

	public interface DbStructureLookup
	{
		 IEnumerator<Pair<int, string>> Labels();
		 IEnumerator<Pair<int, string>> Properties();
		 IEnumerator<Pair<int, string>> RelationshipTypes();

		 IEnumerator<Pair<string[], string[]>> KnownIndices();
		 IEnumerator<Pair<string[], string[]>> KnownUniqueIndices();
		 IEnumerator<Pair<string, string[]>> KnownUniqueConstraints();
		 IEnumerator<Pair<string, string[]>> KnownNodePropertyExistenceConstraints();
		 IEnumerator<Pair<string, string[]>> KnownRelationshipPropertyExistenceConstraints();
		 IEnumerator<Pair<string, string[]>> KnownNodeKeyConstraints();

		 long NodesAllCardinality();
		 long NodesWithLabelCardinality( int labelId );
		 long CardinalityByLabelsAndRelationshipType( int fromLabelId, int relTypeId, int toLabelId );
		 double IndexUniqueValueSelectivity( int labelId, params int[] propertyKeyIds );
		 double IndexPropertyExistsSelectivity( int labelId, params int[] propertyKeyIds );
	}

}