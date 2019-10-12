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
namespace Neo4Net.Kernel.Impl.Api
{

	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IOUtils = Neo4Net.Io.IOUtils;
	using AuxiliaryTransactionState = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using AuxiliaryTransactionStateCloseException = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateCloseException;
	using AuxiliaryTransactionStateHolder = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateHolder;
	using AuxiliaryTransactionStateManager = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using AuxiliaryTransactionStateProvider = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateProvider;
	using Neo4Net.Kernel.impl.util;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;

	public class KernelAuxTransactionStateManager : AuxiliaryTransactionStateManager
	{
		 private volatile CopyOnWriteHashMap<object, AuxiliaryTransactionStateProvider> _providers;

		 public KernelAuxTransactionStateManager()
		 {
			  _providers = new CopyOnWriteHashMap<object, AuxiliaryTransactionStateProvider>();
		 }

		 public override void RegisterProvider( AuxiliaryTransactionStateProvider provider )
		 {
			  _providers[provider.IdentityKey] = provider;
		 }

		 public override void UnregisterProvider( AuxiliaryTransactionStateProvider provider )
		 {
			  _providers.Remove( provider.IdentityKey );
		 }

		 public override AuxiliaryTransactionStateHolder OpenStateHolder()
		 {
			  return new AuxStateHolder( _providers.snapshot() );
		 }

		 private class AuxStateHolder : AuxiliaryTransactionStateHolder, System.Func<object, AuxiliaryTransactionState>
		 {
			  internal readonly IDictionary<object, AuxiliaryTransactionStateProvider> Providers;
			  internal readonly IDictionary<object, AuxiliaryTransactionState> OpenedStates;

			  internal AuxStateHolder( IDictionary<object, AuxiliaryTransactionStateProvider> providers )
			  {
					this.Providers = providers;
					OpenedStates = new Dictionary<object, AuxiliaryTransactionState>();
			  }

			  public override AuxiliaryTransactionState GetState( object providerIdentityKey )
			  {
					return OpenedStates.computeIfAbsent( providerIdentityKey, this ); // Calls out to #apply(Object).
			  }

			  public override AuxiliaryTransactionState Apply( object providerIdentityKey )
			  {
					AuxiliaryTransactionStateProvider provider = Providers[providerIdentityKey];
					if ( provider != null )
					{
						 return provider.CreateNewAuxiliaryTransactionState();
					}
					return null;
			  }

			  public override bool HasChanges()
			  {
					if ( OpenedStates.Count == 0 )
					{
						 return false;
					}
					foreach ( AuxiliaryTransactionState state in OpenedStates.Values )
					{
						 if ( state.HasChanges() )
						 {
							  return true;
						 }
					}
					return false;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void extractCommands(java.util.Collection<org.neo4j.storageengine.api.StorageCommand> extractedCommands) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
			  public override void ExtractCommands( ICollection<StorageCommand> extractedCommands )
			  {
					foreach ( AuxiliaryTransactionState state in OpenedStates.Values )
					{
						 if ( state.HasChanges() )
						 {
							  state.ExtractCommands( extractedCommands );
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.api.txstate.auxiliary.AuxiliaryTransactionStateCloseException
			  public override void Close()
			  {
					IOUtils.close( ( msg, cause ) => new AuxiliaryTransactionStateCloseException( "Failure when closing auxiliary transaction state.", cause ), OpenedStates.Values );
			  }
		 }
	}

}