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
namespace Org.Neo4j.Storageengine.Api.schema
{
	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Resource = Org.Neo4j.Graphdb.Resource;

	/// <summary>
	/// Reader of a label scan store which contains label-->nodes mappings.
	/// </summary>
	public interface LabelScanReader : Resource
	{
		 /// <summary>
		 /// Used as a marker to ignore the "fromId" in calls to <seealso cref="nodesWithAnyOfLabels(long, int[])"/>.
		 /// </summary>

		 /// <param name="labelId"> label token id. </param>
		 /// <returns> node ids with the given {@code labelId}. </returns>
		 PrimitiveLongResourceIterator NodesWithLabel( int labelId );

		 /// <summary>
		 /// Sets the client up for a label scan on <code>labelId</code>
		 /// </summary>
		 /// <param name="client"> the client to communicate with </param>
		 /// <param name="labelId"> label token id </param>
		 void NodesWithLabel( IndexProgressor_NodeLabelClient client, int labelId );

		 /// <param name="labelIds"> label token ids. </param>
		 /// <returns> node ids with any of the given label ids. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default org.neo4j.collection.PrimitiveLongResourceIterator nodesWithAnyOfLabels(int[] labelIds)
	//	 {
	//		  return nodesWithAnyOfLabels(NO_ID, labelIds);
	//	 }

		 /// <param name="fromId"> entity id to start at, exclusive, i.e. the given {@code fromId} will not be included in the result. </param>
		 /// <param name="labelIds"> label token ids. </param>
		 /// <returns> node ids with any of the given label ids. </returns>
		 PrimitiveLongResourceIterator NodesWithAnyOfLabels( long fromId, int[] labelIds );

		 /// <param name="labelIds"> label token ids. </param>
		 /// <returns> node ids with all of the given label ids. </returns>
		 PrimitiveLongResourceIterator NodesWithAllLabels( int[] labelIds );
	}

	public static class LabelScanReader_Fields
	{
		 public const long NO_ID = -1;
	}

}