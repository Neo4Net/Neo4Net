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
namespace Neo4Net.Storageengine.Api
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.CommandVersion.AFTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.CommandVersion.BEFORE;

	/// <summary>
	/// Mode of <seealso cref="StorageEngine.apply(CommandsToApply, TransactionApplicationMode) applying transactions"/>.
	/// Depending on how transaction state have been built, additional work may need to be performed during
	/// application of it.
	/// </summary>
	public sealed class TransactionApplicationMode
	{
		 /// <summary>
		 /// Transaction that is created in the "normal" way and has changed transaction state, which goes
		 /// to commit and produces commands from that. Such a transaction is able to alter cache since it has
		 /// all such high level information directly from the transaction state.
		 /// </summary>
		 public static readonly TransactionApplicationMode Internal = new TransactionApplicationMode( "Internal", InnerEnum.Internal, false, false, false, true, AFTER );

		 /// <summary>
		 /// Transaction that comes from an external source and consists only of commands, i.e. it may not
		 /// contain enough information to f.ex. update cache, but applies to the store just like an internal
		 /// transaction does.
		 /// </summary>
		 public static readonly TransactionApplicationMode External = new TransactionApplicationMode( "External", InnerEnum.External, true, true, false, true, AFTER );

		 /// <summary>
		 /// Transaction that is recovered, where commands are read, much like <seealso cref="EXTERNAL"/>, but should
		 /// be applied differently where extra care should be taken to ensure idempotency. This is because
		 /// a recovered transaction may have already been applied previously to the store.
		 /// </summary>
		 public static readonly TransactionApplicationMode Recovery = new TransactionApplicationMode( "Recovery", InnerEnum.Recovery, true, true, true, true, AFTER );

		 /// <summary>
		 /// Transaction that is recovered during a phase of reverse recovery in order to rewind neo store back
		 /// to a state where forward recovery then can commence from. Rewinding the store back to the point
		 /// if the last checkpoint will allow for correct updates to indexes, because indexes reads from
		 /// a mix of log and store to produce its updates.
		 /// </summary>
		 public static readonly TransactionApplicationMode ReverseRecovery = new TransactionApplicationMode( "ReverseRecovery", InnerEnum.ReverseRecovery, false, false, true, false, BEFORE );

		 private static readonly IList<TransactionApplicationMode> valueList = new List<TransactionApplicationMode>();

		 static TransactionApplicationMode()
		 {
			 valueList.Add( Internal );
			 valueList.Add( External );
			 valueList.Add( Recovery );
			 valueList.Add( ReverseRecovery );
		 }

		 public enum InnerEnum
		 {
			 Internal,
			 External,
			 Recovery,
			 ReverseRecovery
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;

		 internal TransactionApplicationMode( string name, InnerEnum innerEnum, bool needsHighIdTracking, bool needsCacheInvalidation, bool ensureIdempotency, bool indexesAndCounts, CommandVersion version )
		 {
			  this._needsHighIdTracking = needsHighIdTracking;
			  this._needsCacheInvalidation = needsCacheInvalidation;
			  this._needsIdempotencyChecks = ensureIdempotency;
			  this._indexesAndCounts = indexesAndCounts;
			  this._version = version;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <returns> whether or not applying a transaction need to track and update high ids of underlying stores. </returns>
		 public bool NeedsHighIdTracking()
		 {
			  return _needsHighIdTracking;
		 }

		 /// <returns> whether or not applying a transaction need to do additional work of invalidating affected caches. </returns>
		 public bool NeedsCacheInvalidationOnUpdates()
		 {
			  return _needsCacheInvalidation;
		 }

		 /// <returns> whether or not applying a transaction need to be extra cautious about idempotency. </returns>
		 public bool NeedsIdempotencyChecks()
		 {
			  return _needsIdempotencyChecks;
		 }

		 /// <returns> whether or not to include auxiliary stores, such as indexing, counts and statistics. </returns>
		 public bool NeedsAuxiliaryStores()
		 {
			  return _indexesAndCounts;
		 }

		 /// <returns> which version of commands to apply, where some commands have before/after versions. </returns>
		 public CommandVersion Version()
		 {
			  return _version;
		 }

		public static IList<TransactionApplicationMode> values()
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

		public static TransactionApplicationMode valueOf( string name )
		{
			foreach ( TransactionApplicationMode enumInstance in TransactionApplicationMode.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}