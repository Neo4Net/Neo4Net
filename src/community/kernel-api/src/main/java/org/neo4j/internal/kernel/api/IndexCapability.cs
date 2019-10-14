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
namespace Neo4Net.Internal.Kernel.Api
{
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;

	/// <summary>
	/// Capabilities of an index.
	/// Capabilities of an index can not change during the indexes lifetimes.
	/// Caching of IndexCapability is allowed.
	/// It does NOT describe the capabilities of the index at some given moment. For example it does not describe
	/// index state. Rather it describe the functionality that index provide given that it is available.
	/// </summary>
	public interface IndexCapability
	{

		 /// <summary>
		 /// What possible orderings is this index capable to provide for a query on given combination of <seealso cref="ValueCategory"/>.
		 /// Ordering of ValueCategory correspond to ordering of related <seealso cref="IndexReference.properties()"/>.
		 /// </summary>
		 /// <param name="valueCategories"> Ordered array of <seealso cref="ValueCategory ValueCategories"/> for which index should be queried. Note that valueCategory
		 /// must correspond to related <seealso cref="IndexReference.properties()"/>. A {@code null} value in the array
		 /// ({@code new ValueCategory[]{null}}) is interpreted as a wildcard for any <seealso cref="ValueCategory"/>. Note that this is not the same as
		 /// {@code order(null)} which is undefined. </param>
		 /// <returns> <seealso cref="IndexOrder"/> array containing all possible orderings for provided value categories or empty array if no explicit
		 /// ordering is possible or if length of {@code valueCategories} and <seealso cref="IndexReference.properties()"/> differ. </returns>
		 IndexOrder[] OrderCapability( params ValueCategory[] valueCategories );

		 /// <summary>
		 /// Is the index capable of providing values for a query on given combination of <seealso cref="ValueCategory"/>.
		 /// Ordering of ValueCategory correspond to ordering of {@code properties} in related <seealso cref="IndexReference"/>.
		 /// </summary>
		 /// <param name="valueCategories"> Ordered array of <seealso cref="ValueCategory ValueCategories"/> for which index should be queried. Note that valueCategory
		 /// must correspond to related <seealso cref="IndexReference.properties()"/>. <seealso cref="ValueCategory.UNKNOWN"/> can be used as a wildcard for
		 /// any <seealso cref="ValueCategory"/>. Behaviour is undefined for empty {@code null} array and {@code null} values in array. </param>
		 /// <returns> <seealso cref="IndexValueCapability.YES"/> if index is capable of providing values for query on provided array of value categories,
		 /// <seealso cref="IndexValueCapability.NO"/> if not or <seealso cref="IndexValueCapability.PARTIAL"/> for some results. If length of
		 /// {@code valueCategories} and <seealso cref="IndexReference.properties()"/> differ <seealso cref="IndexValueCapability.NO"/> is returned. </returns>
		 IndexValueCapability ValueCapability( params ValueCategory[] valueCategories );

		 /// <summary>
		 /// Fulltext indexes have many restrictions and special capabilities that means they are not substitudes for general indexes, and therefor
		 /// should not be planned to be used for IndexSeeks, for instance.
		 /// <para>
		 /// Perhaps in a future version we will change this into an "index kind" enum or something, but this is all we need for now.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> {@code true} if this index is a fulltext schema index, {@code false} otherwise. </returns>
		 bool FulltextIndex { get; }

		 /// <summary>
		 /// It is possible for some indexes to be <em>eventually consistent</em>, meaning that they might not reflect newly committed changes.
		 /// </summary>
		 /// <returns> {@code true} if this index is eventually consistent, {@code false} otherwise. </returns>
		 bool EventuallyConsistent { get; }

		 /// <returns> an array of limitations that this index has. It could be anything that planning could look at and
		 /// either try to avoid or issue warning for. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default IndexLimitation[] limitations()
	//	 {
	//		  return LIMITIATION_NONE;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean singleWildcard(org.neo4j.values.storable.ValueCategory[] valueCategories)
	//	 {
	//		  return valueCategories.length == 1 && valueCategories[0] == ValueCategory.UNKNOWN;
	//	 }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 IndexCapability NO_CAPABILITY = new IndexCapability()
	//	 {
	//		  @@Override public IndexOrder[] orderCapability(ValueCategory... valueCategories)
	//		  {
	//				return ORDER_NONE;
	//		  }
	//
	//		  @@Override public IndexValueCapability valueCapability(ValueCategory... valueCategories)
	//		  {
	//				return IndexValueCapability.NO;
	//		  }
	//
	//		  @@Override public boolean isFulltextIndex()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public boolean isEventuallyConsistent()
	//		  {
	//				return false;
	//		  }
	//	 };
	}

	public static class IndexCapability_Fields
	{
		 public static readonly IndexOrder[] OrderBoth = new IndexOrder[] { IndexOrder.Ascending, IndexOrder.Descending };
		 public static readonly IndexOrder[] OrderNone = new IndexOrder[0];
		 public static readonly IndexLimitation[] LimitiationNone = new IndexLimitation[0];
	}

}