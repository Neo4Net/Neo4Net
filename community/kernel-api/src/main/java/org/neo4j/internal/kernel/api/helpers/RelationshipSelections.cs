﻿using System;

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
namespace Org.Neo4j.@internal.Kernel.Api.helpers
{
	using Org.Neo4j.Graphdb;

	/// <summary>
	/// Utilities for dealing with RelationshipSelectionCursor and corresponding iterators.
	/// </summary>
	public sealed class RelationshipSelections
	{
		 internal const long UNINITIALIZED = -2L;
		 internal const long NO_ID = -1L;

		 private RelationshipSelections()
		 {
			  throw new System.NotSupportedException( "Do not instantiate" );
		 }

		 /// <summary>
		 /// Returns an outgoing selection cursor given the provided node cursor and relationship types.
		 /// </summary>
		 /// <param name="cursors"> A cursor factor used for allocating the needed cursors </param>
		 /// <param name="node"> A node cursor positioned at the current node. </param>
		 /// <param name="types"> The types of the relationship </param>
		 /// <returns> A cursor that allows traversing the relationship chain. </returns>
		 public static RelationshipSelectionCursor OutgoingCursor( CursorFactory cursors, NodeCursor node, int[] types )
		 {
			  if ( node.Dense )
			  {
					RelationshipDenseSelectionCursor selectionCursor = new RelationshipDenseSelectionCursor();
					SetupOutgoingDense( selectionCursor, cursors, node, types );
					return selectionCursor;
			  }
			  else
			  {
					RelationshipSparseSelectionCursor selectionCursor = new RelationshipSparseSelectionCursor();
					SetupOutgoingSparse( selectionCursor, cursors, node, types );
					return selectionCursor;
			  }
		 }

		 /// <summary>
		 /// Returns an incoming selection cursor given the provided node cursor and relationship types.
		 /// </summary>
		 /// <param name="cursors"> A cursor factor used for allocating the needed cursors </param>
		 /// <param name="node"> A node cursor positioned at the current node. </param>
		 /// <param name="types"> The types of the relationship </param>
		 /// <returns> A cursor that allows traversing the relationship chain. </returns>
		 public static RelationshipSelectionCursor IncomingCursor( CursorFactory cursors, NodeCursor node, int[] types )
		 {
			  if ( node.Dense )
			  {
					RelationshipDenseSelectionCursor selectionCursor = new RelationshipDenseSelectionCursor();
					SetupIncomingDense( selectionCursor, cursors, node, types );
					return selectionCursor;
			  }
			  else
			  {
					RelationshipSparseSelectionCursor selectionCursor = new RelationshipSparseSelectionCursor();
					SetupIncomingSparse( selectionCursor, cursors, node, types );
					return selectionCursor;
			  }
		 }

		 /// <summary>
		 /// Returns a multi-directed selection cursor given the provided node cursor and relationship types.
		 /// </summary>
		 /// <param name="cursors"> A cursor factor used for allocating the needed cursors </param>
		 /// <param name="node"> A node cursor positioned at the current node. </param>
		 /// <param name="types"> The types of the relationship </param>
		 /// <returns> A cursor that allows traversing the relationship chain. </returns>
		 public static RelationshipSelectionCursor AllCursor( CursorFactory cursors, NodeCursor node, int[] types )
		 {
			  if ( node.Dense )
			  {
					RelationshipDenseSelectionCursor selectionCursor = new RelationshipDenseSelectionCursor();
					SetupAllDense( selectionCursor, cursors, node, types );
					return selectionCursor;
			  }
			  else
			  {
					RelationshipSparseSelectionCursor selectionCursor = new RelationshipSparseSelectionCursor();
					SetupAllSparse( selectionCursor, cursors, node, types );
					return selectionCursor;
			  }
		 }

		 /// <summary>
		 /// Returns an outgoing resource iterator given the provided node cursor, direction and relationship types.
		 /// </summary>
		 /// <param name="cursors"> A cursor factor used for allocating the needed cursors </param>
		 /// <param name="node"> A node cursor positioned at the current node. </param>
		 /// <param name="types"> The types of the relationship </param>
		 /// <param name="factory"> factory for creating instance of generic type T </param>
		 /// <returns> An iterator that allows traversing the relationship chain. </returns>
		 public static ResourceIterator<T> OutgoingIterator<T>( CursorFactory cursors, NodeCursor node, int[] types, RelationshipFactory<T> factory )
		 {
			  if ( node.Dense )
			  {
					RelationshipDenseSelectionIterator<T> selectionIterator = new RelationshipDenseSelectionIterator<T>( factory );
					SetupOutgoingDense( selectionIterator, cursors, node, types );
					return selectionIterator;
			  }
			  else
			  {
					RelationshipSparseSelectionIterator<T> selectionIterator = new RelationshipSparseSelectionIterator<T>( factory );
					SetupOutgoingSparse( selectionIterator, cursors, node, types );
					return selectionIterator;
			  }
		 }

		 /// <summary>
		 /// Returns an incoming resource iterator given the provided node cursor, direction and relationship types.
		 /// </summary>
		 /// <param name="cursors"> A cursor factor used for allocating the needed cursors </param>
		 /// <param name="node"> A node cursor positioned at the current node. </param>
		 /// <param name="types"> The types of the relationship </param>
		 /// <param name="factory"> factory for creating instance of generic type T </param>
		 /// <returns> An iterator that allows traversing the relationship chain. </returns>
		 public static ResourceIterator<T> IncomingIterator<T>( CursorFactory cursors, NodeCursor node, int[] types, RelationshipFactory<T> factory )
		 {
			  if ( node.Dense )
			  {
					RelationshipDenseSelectionIterator<T> selectionIterator = new RelationshipDenseSelectionIterator<T>( factory );
					SetupIncomingDense( selectionIterator, cursors, node, types );
					return selectionIterator;
			  }
			  else
			  {
					RelationshipSparseSelectionIterator<T> selectionIterator = new RelationshipSparseSelectionIterator<T>( factory );
					SetupIncomingSparse( selectionIterator, cursors, node, types );
					return selectionIterator;
			  }
		 }

		 /// <summary>
		 /// Returns a multi-directed resource iterator given the provided node cursor, direction and relationship types.
		 /// </summary>
		 /// <param name="cursors"> A cursor factor used for allocating the needed cursors </param>
		 /// <param name="node"> A node cursor positioned at the current node. </param>
		 /// <param name="types"> The types of the relationship </param>
		 /// <param name="factory"> factory for creating instance of generic type T </param>
		 /// <returns> An iterator that allows traversing the relationship chain. </returns>
		 public static ResourceIterator<T> AllIterator<T>( CursorFactory cursors, NodeCursor node, int[] types, RelationshipFactory<T> factory )
		 {
			  if ( node.Dense )
			  {
					RelationshipDenseSelectionIterator<T> selectionIterator = new RelationshipDenseSelectionIterator<T>( factory );
					SetupAllDense( selectionIterator, cursors, node, types );
					return selectionIterator;
			  }
			  else
			  {
					RelationshipSparseSelectionIterator<T> selectionIterator = new RelationshipSparseSelectionIterator<T>( factory );
					SetupAllSparse( selectionIterator, cursors, node, types );
					return selectionIterator;
			  }
		 }

		 private static void SetupOutgoingDense( RelationshipDenseSelection denseSelection, CursorFactory cursors, NodeCursor node, int[] types )
		 {

			  RelationshipGroupCursor groupCursor = cursors.AllocateRelationshipGroupCursor();
			  RelationshipTraversalCursor traversalCursor = cursors.AllocateRelationshipTraversalCursor();
			  try
			  {
					node.Relationships( groupCursor );
					denseSelection.Outgoing( groupCursor, traversalCursor, types );
			  }
			  catch ( Exception t )
			  {
					groupCursor.close();
					traversalCursor.close();
					throw t;
			  }
		 }

		 private static void SetupIncomingDense( RelationshipDenseSelection denseSelection, CursorFactory cursors, NodeCursor node, int[] types )
		 {

			  RelationshipGroupCursor groupCursor = cursors.AllocateRelationshipGroupCursor();
			  RelationshipTraversalCursor traversalCursor = cursors.AllocateRelationshipTraversalCursor();
			  try
			  {
					node.Relationships( groupCursor );
					denseSelection.Incoming( groupCursor, traversalCursor, types );
			  }
			  catch ( Exception t )
			  {
					groupCursor.close();
					traversalCursor.close();
					throw t;
			  }
		 }

		 private static void SetupAllDense( RelationshipDenseSelection denseSelection, CursorFactory cursors, NodeCursor node, int[] types )
		 {

			  RelationshipGroupCursor groupCursor = cursors.AllocateRelationshipGroupCursor();
			  RelationshipTraversalCursor traversalCursor = cursors.AllocateRelationshipTraversalCursor();
			  try
			  {
					node.Relationships( groupCursor );
					denseSelection.All( groupCursor, traversalCursor, types );
			  }
			  catch ( Exception t )
			  {
					groupCursor.close();
					traversalCursor.close();
					throw t;
			  }
		 }

		 private static void SetupOutgoingSparse( RelationshipSparseSelection sparseSelection, CursorFactory cursors, NodeCursor node, int[] types )
		 {
			  RelationshipTraversalCursor traversalCursor = cursors.AllocateRelationshipTraversalCursor();
			  try
			  {
					node.AllRelationships( traversalCursor );
					sparseSelection.Outgoing( traversalCursor, types );
			  }
			  catch ( Exception t )
			  {
					traversalCursor.close();
					throw t;
			  }
		 }

		 private static void SetupIncomingSparse( RelationshipSparseSelection sparseSelection, CursorFactory cursors, NodeCursor node, int[] types )
		 {
			  RelationshipTraversalCursor traversalCursor = cursors.AllocateRelationshipTraversalCursor();
			  try
			  {
					node.AllRelationships( traversalCursor );
					sparseSelection.Incoming( traversalCursor, types );
			  }
			  catch ( Exception t )
			  {
					traversalCursor.close();
					throw t;
			  }
		 }

		 private static void SetupAllSparse( RelationshipSparseSelection sparseSelection, CursorFactory cursors, NodeCursor node, int[] types )
		 {
			  RelationshipTraversalCursor traversalCursor = cursors.AllocateRelationshipTraversalCursor();
			  try
			  {
					node.AllRelationships( traversalCursor );
					sparseSelection.All( traversalCursor, types );
			  }
			  catch ( Exception t )
			  {
					traversalCursor.close();
					throw t;
			  }
		 }
	}

}