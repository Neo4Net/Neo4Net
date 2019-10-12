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
namespace Org.Neo4j.upgrade
{
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using HighLimit = Org.Neo4j.Kernel.impl.store.format.highlimit.HighLimit;
	using StoreUpgraderTest = Org.Neo4j.Kernel.impl.storemigration.StoreUpgraderTest;

	/// <summary>
	/// Runs the store upgrader tests from older versions, migrating to the current enterprise version.
	/// </summary>
	public class StandardToEnterpriseStoreUpgraderTest : StoreUpgraderTest
	{
		 public StandardToEnterpriseStoreUpgraderTest( RecordFormats recordFormats ) : base( recordFormats )
		 {
		 }

		 protected internal override RecordFormats RecordFormats
		 {
			 get
			 {
				  return HighLimit.RECORD_FORMATS;
			 }
		 }

		 protected internal override string RecordFormatsName
		 {
			 get
			 {
				  return HighLimit.NAME;
			 }
		 }
	}

}