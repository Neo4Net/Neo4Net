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
namespace Neo4Net.Kernel.impl.store.format
{
	using Service = Neo4Net.Helpers.Service;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using MetaDataRecord = Neo4Net.Kernel.Impl.Store.Records.MetaDataRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

	/// <summary>
	/// The record formats that a store version uses. Contains all formats for all different stores as well as
	/// accessors for which <seealso cref="Capability capabilities"/> a format has as to be able to compare between formats.
	/// </summary>
	public interface RecordFormats
	{

		 string StoreVersion();

		 /// <returns> the Neo4Net version where this format was introduced. It is almost certainly NOT the only version of
		 /// Neo4Net where this format is used. </returns>
		 string IntroductionVersion();

		 /// <summary>
		 /// Generation of this format, format family local int value which should be incrementing along with
		 /// releases, e.g. store version, e.g. official versions of the product. Use to determine generation of particular
		 /// format and to be able to find newest of among them.
		 /// When implementing new format generation can be assigned to any positive integer, but please take into account
		 /// future version generations.
		 /// When evolving an older format the generation of the new format version should
		 /// be higher than the format it evolves from.
		 /// The generation value doesn't need to correlate to any other value, the only thing needed is to
		 /// determine "older" or "newer".
		 /// </summary>
		 /// <returns> format generation, with the intent of usage being that a store can migrate to a newer or
		 /// same generation, but not to an older generation within same format family. </returns>
		 int Generation();

		 RecordFormat<NodeRecord> Node();

		 RecordFormat<RelationshipGroupRecord> RelationshipGroup();

		 RecordFormat<RelationshipRecord> Relationship();

		 RecordFormat<PropertyRecord> Property();

		 RecordFormat<LabelTokenRecord> LabelToken();

		 RecordFormat<PropertyKeyTokenRecord> PropertyKeyToken();

		 RecordFormat<RelationshipTypeTokenRecord> RelationshipTypeToken();

		 RecordFormat<DynamicRecord> Dynamic();

		 RecordFormat<MetaDataRecord> MetaData();

		 /// <summary>
		 /// Use when comparing one format to another, for example for migration purposes.
		 /// </summary>
		 /// <returns> array of <seealso cref="Capability capabilities"/> for comparison. </returns>
		 Capability[] Capabilities();

		 /// <param name="capability"> <seealso cref="Capability"/> to check for. </param>
		 /// <returns> whether or not this format has a certain <seealso cref="Capability"/>. </returns>
		 bool HasCapability( Capability capability );

		 /// <summary>
		 /// Get format family to which this format belongs to. </summary>
		 /// <returns> format family </returns>
		 /// <seealso cref= FormatFamily </seealso>
		 FormatFamily FormatFamily { get; }

		 /// <summary>
		 /// Whether or not changes in the {@code other} format, compared to this format, for the given {@code type}, are compatible.
		 /// </summary>
		 /// <param name="other"> <seealso cref="RecordFormats"/> to check compatibility with. </param>
		 /// <param name="type"> <seealso cref="CapabilityType type"/> of capability to check compatibility for. </param>
		 /// <returns> true if the {@code other} format have compatible capabilities of the given {@code type}. </returns>
		 bool HasCompatibleCapabilities( RecordFormats other, CapabilityType type );

		 /// <summary>
		 /// Record format name </summary>
		 /// <returns> name of record format </returns>
		 string Name();
	}

	 public abstract class RecordFormats_Factory : Service
	 {
		  public RecordFormats_Factory( string key, params string[] altKeys ) : base( key, altKeys )
		  {
		  }

		  public abstract RecordFormats NewInstance();
	 }

}