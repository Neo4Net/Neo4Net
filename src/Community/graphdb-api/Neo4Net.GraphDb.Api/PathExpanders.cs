using System.Collections.Generic;
using System.Diagnostics;

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

namespace Neo4Net.GraphDb
{
   using Neo4Net.GraphDb.Traversal;
   using Paths = Neo4Net.GraphDb.Traversal.Paths;
   using StandardExpander = Neo4Net.GraphDb.impl.StandardExpander;

   /// <summary>
   /// A catalog of convenient <seealso cref="PathExpander"/> factory methods.
   /// <para>
   /// Use <seealso cref="PathExpanderBuilder"/> to build specialized <seealso cref="PathExpander"/>s.
   /// </para>
   /// </summary>
   public abstract class PathExpanders
   {
      /// <summary>
      /// A very permissive <seealso cref="PathExpander"/> that follows any type in any direction.
      /// </summary>
      /// @param <STATE> the type of the object that holds the state </param>
      /// <returns> a very permissive <seealso cref="PathExpander"/> that follows any type in any direction </returns>
      public static IPathExpander<STATE> AllTypesAndDirections<STATE>()
      {
         return StandardExpander.DEFAULT;
      }

      /// <summary>
      /// A very permissive <seealso cref="PathExpander"/> that follows {@code type} relationships in any direction.
      /// </summary>
      /// <param name="type"> the type of relationships to expand in any direction </param>
      /// @param <STATE> the type of the object that holds the state </param>
      /// <returns> a very permissive <seealso cref="PathExpander"/> that follows {@code type} relationships in any direction </returns>

      public static IPathExpander<STATE> ForType<STATE>(IRelationshipType type)
      {
         return StandardExpander.create(type, Direction.Both);
      }

      /// <summary>
      /// A very permissive <seealso cref="PathExpander"/> that follows any type in {@code direction}.
      /// </summary>
      /// <param name="direction"> the direction to follow relationships in </param>
      /// @param <STATE> the type of the object that holds the state </param>
      /// <returns> a very permissive <seealso cref="PathExpander"/> that follows any type in {@code direction} </returns>

      public static IPathExpander<STATE> ForDirection<STATE>(Direction direction)
      {
         return StandardExpander.create(direction);
      }

      /// <summary>
      /// A very restricted <seealso cref="PathExpander"/> that follows {@code type} in {@code direction}.
      /// </summary>
      /// <param name="type"> the type of relationships to follow </param>
      /// <param name="direction"> the direction to follow relationships in </param>
      /// @param <STATE> the type of the object that holds the state </param>
      /// <returns> a very restricted <seealso cref="PathExpander"/> that follows {@code type} in {@code direction} </returns>
      public static IPathExpander<STATE> ForTypeAndDirection<STATE>(IRelationshipType type, Direction direction)
      {
         return StandardExpander.create(type, direction);
      }

      /// <summary>
      /// A very restricted <seealso cref="PathExpander"/> that follows only the {@code type}/{@code direction} pairs that you list.
      /// </summary>
      /// <param name="type1"> the type of relationships to follow in {@code direction1} </param>
      /// <param name="direction1"> the direction to follow {@code type1} relationships in </param>
      /// <param name="type2"> the type of relationships to follow in {@code direction2} </param>
      /// <param name="direction2"> the direction to follow {@code type2} relationships in </param>
      /// <param name="more"> add more {@code type}/{@code direction} pairs </param>
      /// @param <STATE> the type of the object that holds the state </param>
      /// <returns> a very restricted <seealso cref="PathExpander"/> that follows only the {@code type}/{@code direction} pairs that you list </returns>
      public static IPathExpander<STATE> ForTypesAndDirections<STATE>(IRelationshipType type1, Direction direction1, IRelationshipType type2, Direction direction2, params object[] more)
      {
         return StandardExpander.create(type1, direction1, type2, direction2, more);
      }

      /// <summary>
      /// An expander forcing constant relationship direction
      /// </summary>
      /// <param name="types"> types of relationships to follow </param>
      /// @param <STATE> the type of the object that holds the state </param>
      /// <returns> a <seealso cref="PathExpander"/> which enforces constant relationship direction </returns>
      public static IPathExpander<STATE> ForConstantDirectionWithTypes<STATE>(params IRelationshipType[] types)
      {
         return new PathExpanderAnonymousInnerClass(types);
      }

      private class PathExpanderAnonymousInnerClass : IPathExpander<STATE>
      {
         private Neo4Net.GraphDb.IRelationshipType[] _types;

         public PathExpanderAnonymousInnerClass(Neo4Net.GraphDb.IRelationshipType[] types)
         {
            _types = types;
         }

         public IEnumerable<IRelationship> expand(IPath path, IBranchState<STATE> state)
         {
            if (path.Length == 0)
            {
               return path.EndNode.getRelationships(_types);
            }
            else
            {
               Direction direction = getDirectionOfLastRelationship(path);
               return path.EndNode.getRelationships(direction, _types);
            }
         }

         public IPathExpander<STATE> reverse()
         {
            return this;
         }

         private Direction getDirectionOfLastRelationship(IPath path)
         {
            Debug.Assert(path.Length > 0);
            Direction direction = Direction.Incoming;
            if (path.EndNode.Equals(path.LastRelationship.EndNode))
            {
               direction = Direction.Outgoing;
            }
            return direction;
         }
      }

      private PathExpanders()
      {
         // you should never instantiate this
      }

      /// <summary>
      /// A wrapper that uses <seealso cref="Neo4Net.GraphDb.traversal.Paths.DefaultPathDescriptor"/> to print expanded paths.
      /// All expanded paths will be printed using System.out. </summary>
      /// <param name="source">    <seealso cref="PathExpander"/> to wrap. </param>
      /// @param <STATE>   the type of the object that holds the state </param>
      /// <returns> A new <seealso cref="PathExpander"/>. </returns>
      public static IPathExpander<STATE> PrintingWrapper<STATE>(IPathExpander<STATE> source)
      {
         return PrintingWrapper(source, new Paths.DefaultPathDescriptor());
      }

      /// <summary>
      /// A wrapper that uses <seealso cref="Neo4Net.GraphDb.traversal.Paths.DefaultPathDescriptor"/>
      /// to print expanded paths that fulfill <seealso cref="BiFunction"/> predicate.
      /// Will use System.out as <seealso cref="PrintStream"/>. </summary>
      /// <param name="source">    <seealso cref="PathExpander"/> to wrap. </param>
      /// <param name="pred">      <seealso cref="BiFunction"/> used as predicate for printing expansion. </param>
      /// @param <STATE>   the type of the object that holds the state </param>
      /// <returns>          A new <seealso cref="PathExpander"/>. </returns>
      public static IPathExpander<STATE> PrintingWrapper<STATE>(IPathExpander<STATE> source, System.Func<IPath, IBranchState, bool> pred)
      {
         return PrintingWrapper(source, pred, new Paths.DefaultPathDescriptor());
      }

      /// <summary>
      /// A wrapper that uses <seealso cref="Neo4Net.GraphDb.traversal.Paths.DefaultPathDescriptor"/> to print expanded paths
      /// using given <seealso cref="Neo4Net.GraphDb.traversal.Paths.PathDescriptor"/>.
      /// All expanded paths will be printed.
      /// Will use System.out as <seealso cref="PrintStream"/>. </summary>
      /// <param name="source">        <seealso cref="PathExpander"/> to wrap. </param>
      /// <param name="descriptor">    <seealso cref="Neo4Net.GraphDb.traversal.Paths.PathDescriptor"/> to use when printing paths. </param>
      /// @param <STATE>       the type of the object that holds the state </param>
      /// <returns>              A new <seealso cref="PathExpander"/>. </returns>
      public static IPathExpander<STATE> PrintingWrapper<STATE>(IPathExpander<STATE> source, Paths.IPathDescriptor descriptor)
      {
         return PrintingWrapper(source, (IPropertyContainers, stateBranchState) => true, descriptor);
      }

      /// <summary>
      /// A wrapper that uses <seealso cref="Neo4Net.GraphDb.traversal.Paths.DefaultPathDescriptor"/> to print expanded paths
      /// that fulfill <seealso cref="BiFunction"/> predicate using given <seealso cref="Neo4Net.GraphDb.traversal.Paths.PathDescriptor"/>.
      /// Will use System.out as <seealso cref="PrintStream"/>. </summary>
      /// <param name="source">    <seealso cref="PathExpander"/> to wrap. </param>
      /// <param name="pred">      <seealso cref="BiFunction"/> used as predicate for printing expansion. </param>
      /// <param name="descriptor"> <seealso cref="Neo4Net.GraphDb.traversal.Paths.PathDescriptor"/> to use when printing paths. </param>
      /// @param <STATE>   the type of the object that holds the state </param>
      /// <returns>          A new <seealso cref="PathExpander"/>. </returns>
      public static IPathExpander<STATE> PrintingWrapper<STATE>(IPathExpander<STATE> source, System.Func<IPath, IBranchState, bool> pred, Paths.IPathDescriptor descriptor)
      {
         return PrintingWrapper(source, pred, descriptor, System.out );
      }

      /// <summary>
      /// A wrapper that uses <seealso cref="Neo4Net.GraphDb.traversal.Paths.DefaultPathDescriptor"/> to print expanded paths
      /// that fulfill <seealso cref="BiFunction"/> predicate using given <seealso cref="Neo4Net.GraphDb.traversal.Paths.PathDescriptor"/>. </summary>
      /// <param name="source">        <seealso cref="PathExpander"/> to wrap. </param>
      /// <param name="pred">          <seealso cref="BiFunction"/> used as predicate for printing expansion. </param>
      /// <param name="descriptor">    <seealso cref="Neo4Net.GraphDb.traversal.Paths.PathDescriptor"/> to use when printing paths. </param>
      /// <param name="out">           <seealso cref="PrintStream"/> to use for printing expanded paths </param>
      /// @param <STATE>       the type of the object that holds the state </param>
      /// <returns>              A new <seealso cref="PathExpander"/>. </returns>
      public static IPathExpander<STATE> PrintingWrapper<STATE>(IPathExpander<STATE> source, System.Func<IPath, IBranchState, bool> pred, Paths.IPathDescriptor descriptor, PrintStream @out)
      {
         return new PathExpanderAnonymousInnerClass2(source, pred, descriptor, @out);
      }

      private class PathExpanderAnonymousInnerClass2 : IPathExpander<STATE>
      {
         private Neo4Net.GraphDb.IPathExpander<STATE> _source;
         private System.Func<IPath, IBranchState, bool> _pred;
         private Paths.IPathDescriptor _descriptor;
         private PrintStream @out;

         public PathExpanderAnonymousInnerClass2(Neo4Net.GraphDb.IPathExpander<STATE> source, System.Func<IPath, IBranchState, bool> pred, Paths.IPathDescriptor descriptor, PrintStream @out)
         {
            _source = source;
            _pred = pred;
            _descriptor = descriptor;
            this.@out = @out;
         }

         public IEnumerable<IRelationship> Expand(IPath path, IBranchState state)
         {
            if (_pred(path, state))
            {
               @out.println(Paths.pathToString(path, _descriptor));
            }
            return _source.Expand(path, state);
         }

         public IPathExpander<STATE> Reverse()
         {
            return PrintingWrapper(_source.Reverse(), _pred, _descriptor, @out);
         }
      }
   }
}