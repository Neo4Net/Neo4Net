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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogVersions.CURRENT_LOG_VERSION;

	public class InMemoryVersionableReadableClosablePositionAwareChannel : InMemoryClosableChannel, ReadableLogChannel
	{
		 private readonly long _version;
		 private readonly sbyte _formatVersion;

		 public InMemoryVersionableReadableClosablePositionAwareChannel() : this(0, CURRENT_LOG_VERSION)
		 {
		 }

		 public InMemoryVersionableReadableClosablePositionAwareChannel( long version, sbyte formatVersion )
		 {
			  this._version = version;
			  this._formatVersion = formatVersion;
		 }

		 public virtual long Version
		 {
			 get
			 {
				  return _version;
			 }
		 }

		 public virtual sbyte LogFormatVersion
		 {
			 get
			 {
				  return _formatVersion;
			 }
		 }
	}

}