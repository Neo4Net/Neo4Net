using System;
using System.Collections.Generic;

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
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.Neo4Net.graphdb.DependencyResolver_SelectionStrategy.FIRST;

   /// <summary>
   /// Find a dependency given a type.
   /// </summary>
   public interface IDependencyResolver
   {
      /// <summary>
      /// Tries to resolve a dependency that matches a given class. No specific
      /// <seealso cref="SelectionStrategy"/> is used, so the first encountered matching dependency will be returned.
      ///
      /// </summary>
      /// <param name="type"> the type of <seealso cref="System.Type"/> that the returned instance must implement. </param>
      /// @param <T> the type that the returned instance must implement </param>
      /// <returns> the resolved dependency for the given type. </returns>
      /// <exception cref="IllegalArgumentException"> if no matching dependency was found. </exception>
      /// @deprecated in next major version default selection strategy will be changed to more strict <seealso cref="IDependencyResolver.SelectionStrategy.ONLY"/>

      T ResolveDependency<T>(Type type);

      /// <summary>
      /// Tries to resolve a dependency that matches a given class. All candidates are fed to the
      /// {@code selector} which ultimately becomes responsible for making the choice between all available candidates.
      /// </summary>
      /// <param name="type"> the type of <seealso cref="System.Type"/> that the returned instance must implement. </param>
      /// <param name="selector"> <seealso cref="SelectionStrategy"/> which will make the choice of which one to return among
      /// matching candidates. </param>
      /// @param <T> the type that the returned instance must implement </param>
      /// <returns> the resolved dependency for the given type. </returns>
      /// <exception cref="IllegalArgumentException"> if no matching dependency was found. </exception>

      T ResolveDependency<T>(Type type, IDependencyResolver_SelectionStrategy selector);

      /// <summary>
      /// Tries to resolve a dependencies that matches a given class.
      /// </summary>
      /// <param name="type"> the type of <seealso cref="System.Type"/> that the returned instances must implement. </param>
      /// @param <T> the type that the returned instance must implement </param>
      /// <returns> the list of resolved dependencies for the given type. </returns>
      //JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
      //		 default <T> Iterable<JavaToDotNetGenericWildcard extends T> resolveTypeDependencies(Class<T> type)
      //	 {
      //		  throw new UnsupportedOperationException("not implemented");
      //	 }

      System.Func<T> ProvideDependency<T>(Type type, IDependencyResolver_SelectionStrategy selector);

      System.Func<T> provideDependency<T>(Type type);

      /// <summary>
      /// Responsible for making the choice between available candidates.
      /// </summary>

      /// <summary>
      /// Adapter for <seealso cref="IDependencyResolver"/> which will select the first available candidate by default
      /// for <seealso cref="ResolveDependency(System.Type)"/>.
      /// </summary>
      /// @deprecated in next major version default selection strategy will be changed to more strict <seealso cref="IDependencyResolver.SelectionStrategy.ONLY"/>
   }

   public interface IDependencyResolver_SelectionStrategy
   {
      /// <summary>
      /// Given a set of candidates, select an appropriate one. Even if there are candidates this
      /// method may throw <seealso cref="System.ArgumentException"/> if there was no suitable candidate.
      /// </summary>
      /// <param name="type"> the type of items. </param>
      /// <param name="candidates"> candidates up for selection, where one should be picked. There might
      /// also be no suitable candidate, in which case an exception should be thrown. </param>
      /// @param <T> the type of items </param>
      /// <returns> a suitable candidate among all available. </returns>
      /// <exception cref="IllegalArgumentException"> if no suitable candidate was found. </exception>

      //ORIGINAL LINE: <T> T select(Class<T> type, Iterable<? extends T> candidates) throws IllegalArgumentException;
      //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
      T Select<T, T1>(Type type, IEnumerable<T1> candidates);

      //JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
      //	  DependencyResolver.SelectionStrategy FIRST = new DependencyResolver.SelectionStrategy()
      //	  {
      //			@@Override public <T> T select(Class<T> type, Iterable<? extends T> candidates) throws IllegalArgumentException
      //			{
      //				 Iterator<? extends T> iterator = candidates.iterator();
      //				 if (!iterator.hasNext())
      //				 {
      //					  throw new IllegalArgumentException("Could not resolve dependency of type:" + type.getName());
      //				 }
      //				 return iterator.next();
      //			}
      //	  };

      /// <summary>
      /// Returns the one and only dependency, or throws.
      /// </summary>
      //JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
      //	  DependencyResolver.SelectionStrategy ONLY = new DependencyResolver.SelectionStrategy()
      //	  {
      //			@@Override public <T> T select(Class<T> type, Iterable<? extends T> candidates) throws IllegalArgumentException
      //			{
      //				 Iterator<? extends T> iterator = candidates.iterator();
      //				 if (!iterator.hasNext())
      //				 {
      //					  throw new IllegalArgumentException("Could not resolve dependency of type:" + type.getName());
      //				 }
      //
      //				 T only = iterator.next();
      //
      //				 if (iterator.hasNext())
      //				 {
      //					  throw new IllegalArgumentException("Multiple dependencies of type:" + type.getName());
      //				 }
      //				 else
      //				 {
      //					  return only;
      //				 }
      //			}
      //	  };
   }

   public abstract class DependencyResolver_Adapter : IDependencyResolver
   {
      //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
      //ORIGINAL LINE: public abstract Iterable<JavaToDotNetGenericWildcard extends T> resolveTypeDependencies(Class<T> type);
      public abstract IEnumerable<T> ResolveTypeDependencies(Type type);

      public abstract T ResolveDependency(Type type, IDependencyResolver_SelectionStrategy selector);

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public <T> T resolveDependency(Class<T> type) throws IllegalArgumentException
      public override T ResolveDependency<T>(Type type)
      {
         type = typeof(T);
         return ResolveDependency(type, FIRST);
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public <T> System.Func<T> provideDependency(final Class<T> type, final DependencyResolver_SelectionStrategy selector)
      public override System.Func<T> ProvideDependency<T>(Type type, IDependencyResolver_SelectionStrategy selector)
      {
         type = typeof(T);
         return () => ResolveDependency(type, selector);
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public <T> System.Func<T> provideDependency(final Class<T> type)
      public override System.Func<T> ProvideDependency<T>(Type type)
      {
         type = typeof(T);
         return () => ResolveDependency(type);
      }
   }
}