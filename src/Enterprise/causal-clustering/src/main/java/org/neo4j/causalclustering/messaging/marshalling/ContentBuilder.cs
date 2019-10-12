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
namespace Neo4Net.causalclustering.messaging.marshalling
{

	/// <summary>
	/// Used to lazily build object of given type where the resulting object may contain objects of the same type.
	///  Executes the composed function when <seealso cref="build()"/> is called. </summary>
	/// @param <CONTENT> type of the object that will be built. </param>
	public class ContentBuilder<CONTENT>
	{
		 private bool _isComplete;
		 private System.Func<CONTENT, CONTENT> _contentFunction;

		 public static ContentBuilder<C> EmptyUnfinished<C>()
		 {
			  return new ContentBuilder<C>( content => content, false );
		 }

		 public static ContentBuilder<C> Unfinished<C>( System.Func<C, C> contentFunction )
		 {
			  return new ContentBuilder<C>( contentFunction, false );
		 }

		 public static ContentBuilder<C> Finished<C>( C content )
		 {
			  return new ContentBuilder<C>( ignored => content, true );
		 }

		 private ContentBuilder( System.Func<CONTENT, CONTENT> contentFunction, bool isComplete )
		 {
			  this._contentFunction = contentFunction;
			  this._isComplete = isComplete;
		 }

		 /// <summary>
		 ///  Signals that the object is ready to be built </summary>
		 /// <returns> true if builder is complete and ready to be built. </returns>
		 public virtual bool Complete
		 {
			 get
			 {
				  return _isComplete;
			 }
		 }

		 /// <summary>
		 /// Composes this with the given builder and updates <seealso cref="isComplete()"/> with the provided builder. </summary>
		 /// <param name="contentBuilder"> that will be combined with this builder </param>
		 /// <returns> The combined builder </returns>
		 /// <exception cref="IllegalStateException"> if the current builder is already complete </exception>
		 public virtual ContentBuilder<CONTENT> Combine( ContentBuilder<CONTENT> contentBuilder )
		 {
			  if ( _isComplete )
			  {
					throw new System.InvalidOperationException( "This content builder has already completed and cannot be combined." );
			  }
			  _contentFunction = _contentFunction.compose( contentBuilder._contentFunction );
			  _isComplete = contentBuilder._isComplete;
			  return this;
		 }

		 /// <summary>
		 /// Builds the object given type. Can only be called if <seealso cref="isComplete()"/> is true. </summary>
		 /// <returns> the complete object </returns>
		 /// <exception cref="IllegalStateException"> if <seealso cref="isComplete()"/> is false. </exception>
		 public virtual CONTENT Build()
		 {
			  if ( !_isComplete )
			  {
					throw new System.InvalidOperationException( "Cannot build unfinished content" );
			  }
			  return _contentFunction.apply( null );
		 }
	}

}