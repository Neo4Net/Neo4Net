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
namespace Neo4Net.Kernel.api.explicitindex
{

	using ExplicitIndexWrite = Neo4Net.Internal.Kernel.Api.ExplicitIndexWrite;
	using AutoIndexingKernelException = Neo4Net.Internal.Kernel.Api.exceptions.explicitindex.AutoIndexingKernelException;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Abstract interface for accessing legacy auto indexing facilities for a given type of IEntity (nodes or relationships)
	/// 
	/// Instances have three main concerns:
	/// - Controlling if auto indexing of the underlying IEntity type (node/relationship) is enabled or disabled
	/// - Controlling which properties are being indexed currently
	/// - Tracking updates
	/// </summary>
	/// <seealso cref= AutoIndexing </seealso>
	/// <seealso cref= org.Neo4Net.kernel.impl.api.explicitindex.InternalAutoIndexOperations </seealso>
	public interface AutoIndexOperations
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void propertyAdded(org.Neo4Net.internal.kernel.api.ExplicitIndexWrite write, long IEntityId, int propertyKeyId, org.Neo4Net.values.storable.Value value) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 void PropertyAdded( ExplicitIndexWrite write, long IEntityId, int propertyKeyId, Value value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void propertyChanged(org.Neo4Net.internal.kernel.api.ExplicitIndexWrite write, long IEntityId, int propertyKeyId, org.Neo4Net.values.storable.Value oldValue, org.Neo4Net.values.storable.Value newValue) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 void PropertyChanged( ExplicitIndexWrite write, long IEntityId, int propertyKeyId, Value oldValue, Value newValue );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void propertyRemoved(org.Neo4Net.internal.kernel.api.ExplicitIndexWrite write, long IEntityId, int propertyKey) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 void PropertyRemoved( ExplicitIndexWrite write, long IEntityId, int propertyKey );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void IEntityRemoved(org.Neo4Net.internal.kernel.api.ExplicitIndexWrite write, long IEntityId) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 void IEntityRemoved( ExplicitIndexWrite write, long IEntityId );

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
	//		  @@Override public void propertyAdded(ExplicitIndexWrite write, long IEntityId, int propertyKeyId, Value value)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public void propertyChanged(ExplicitIndexWrite write, long IEntityId, int propertyKeyId, Value oldValue, Value newValue)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public void propertyRemoved(ExplicitIndexWrite write, long IEntityId, int propertyKey)
	//		  {
	//				throw new UnsupportedOperationException();
	//		  }
	//
	//		  @@Override public void IEntityRemoved(ExplicitIndexWrite write, long IEntityId)
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