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
namespace Org.Neo4j.Kernel.impl.transaction.log
{

	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;

	/// <summary>
	/// Basically a sequence of <seealso cref="StoreChannel channels"/> seamlessly seen as one.
	/// </summary>
	public class ReadAheadLogChannel : ReadAheadChannel<LogVersionedStoreChannel>, ReadableLogChannel
	{
		 private readonly LogVersionBridge _bridge;

		 public ReadAheadLogChannel( LogVersionedStoreChannel startingChannel ) : this( startingChannel, LogVersionBridge_Fields.NoMoreChannels, DefaultReadAheadSize )
		 {
		 }

		 public ReadAheadLogChannel( LogVersionedStoreChannel startingChannel, LogVersionBridge bridge ) : this( startingChannel, bridge, DefaultReadAheadSize )
		 {
		 }

		 public ReadAheadLogChannel( LogVersionedStoreChannel startingChannel, LogVersionBridge bridge, int readAheadSize ) : base( startingChannel, readAheadSize )
		 {
			  this._bridge = bridge;
		 }

		 public virtual long Version
		 {
			 get
			 {
				  return Channel.Version;
			 }
		 }

		 public virtual sbyte LogFormatVersion
		 {
			 get
			 {
				  return Channel.LogFormatVersion;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LogPositionMarker getCurrentPosition(LogPositionMarker positionMarker) throws java.io.IOException
		 public override LogPositionMarker GetCurrentPosition( LogPositionMarker positionMarker )
		 {
			  positionMarker.Mark( Channel.Version, Position() );
			  return positionMarker;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected LogVersionedStoreChannel next(LogVersionedStoreChannel channel) throws java.io.IOException
		 protected internal override LogVersionedStoreChannel Next( LogVersionedStoreChannel channel )
		 {
			  return _bridge( channel );
		 }
	}

}