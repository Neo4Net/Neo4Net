using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.api.explicitindex
{

	using ExplicitIndexWrite = Org.Neo4j.@internal.Kernel.Api.ExplicitIndexWrite;
	using AutoIndexingKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.AutoIndexingKernelException;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Abstract interface for accessing legacy auto indexing facilities for a given type of entity (nodes or relationships)
	/// 
	/// Instances have three main concerns:
	/// - Controlling if auto indexing of the underlying entity type (node/relationship) is enabled or disabled
	/// - Controlling which properties are being indexed currently
	/// - Tracking updates
	/// </summary>
	/// <seealso cref= AutoIndexing </seealso>
	/// <seealso cref= org.neo4j.kernel.impl.api.explicitindex.InternalAutoIndexOperations </seealso>
	public interface AutoIndexOperations
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void propertyAdded(org.neo4j.internal.kernel.api.ExplicitIndexWrite write, long entityId, int propertyKeyId, org.neo4j.values.storable.Value value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 void PropertyAdded( ExplicitIndexWrite write, long entityId, int propertyKeyId, Value value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void propertyChanged(org.neo4j.internal.kernel.api.ExplicitIndexWrite write, long entityId, int propertyKeyId, org.neo4j.values.storable.Value oldValue, org.neo4j.values.storable.Value newValue) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 void PropertyChanged( ExplicitIndexWrite write, long entityId, int propertyKeyId, Value oldValue, Value newValue );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void propertyRemoved(org.neo4j.internal.kernel.api.ExplicitIndexWrite write, long entityId, int propertyKey) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 void PropertyRemoved( ExplicitIndexWrite write, long entityId, int propertyKey );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void entityRemoved(org.neo4j.internal.kernel.api.ExplicitIndexWrite write, long entityId) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 void EntityRemoved( ExplicitIndexWrite write, long entityId );

		 bool Enabled();

		 void Enabled( bool enabled );

		 void StartAutoIndexingProperty( string propName );

		 void StopAutoIndexingProperty( string propName );

		 ISet<string> AutoIndexedProperties { get; }

		 /// <summary>
		 /// Instance of <seealso cref="AutoIndexOperations"/> that throws <seealso cref="System.NotSupportedException"/> when any of its methods is invoked
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 AutoIndexOperations UNSUPPORTED = new AutoIndexOperations()
	//	 {
	//
	//		  @@Override public void propertyAdded(ExplicitIndexWrite write, long entityId, int propertyKeyId, Value value)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public void propertyChanged(ExplicitIndexWrite write, long entityId, int propertyKeyId, Value oldValue, Value newValue)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public void propertyRemoved(ExplicitIndexWrite write, long entityId, int propertyKey)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public void entityRemoved(ExplicitIndexWrite write, long entityId)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public boolean enabled()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public void enabled(boolean enabled)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public void startAutoIndexingProperty(String propName)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public void stopAutoIndexingProperty(String propName)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public Set<String> getAutoIndexedProperties()
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//	 };
	}

}