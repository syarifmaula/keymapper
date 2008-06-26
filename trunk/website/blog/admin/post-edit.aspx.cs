﻿using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Text;


namespace KMBlog
{
	public partial class post_edit : System.Web.UI.Page
	{


		protected void Page_Load(object sender, EventArgs e)
		{

			if (Page.IsPostBack)
			{
				if (SavePost())
				{
					Response.Write("Post saved");
				}
				else
				{
					Response.Write("Post not saved");
				}
				return;
			}

			postmonth.Items.Add("January");
			postmonth.Items.Add("February");
			postmonth.Items.Add("March");
			postmonth.Items.Add("April");
			postmonth.Items.Add("May");
			postmonth.Items.Add("June");
			postmonth.Items.Add("July");
			postmonth.Items.Add("August");
			postmonth.Items.Add("September");
			postmonth.Items.Add("October");
			postmonth.Items.Add("November");
			postmonth.Items.Add("December");

			int postID = GetPostIDFromQueryString();

			if (postID != 0)
			{

				hiddenPostID.Value = postID.ToString();

				string cs = ConfigurationManager.ConnectionStrings["default"].ConnectionString;

				using (SqlConnection connection = new SqlConnection(cs))
				{
					if (connection != null)
					{

						connection.Open();
						SqlCommand sc = new SqlCommand();

						sc.Connection = connection;

						sc.CommandText = "GetPostByID";
						sc.Parameters.AddWithValue("PostID", postID);
						sc.CommandType = CommandType.StoredProcedure;

						form1.Style.Add("display", "none");

						using (SqlDataReader dr = sc.ExecuteReader())
						{
							while (dr.Read())
							{
								// Only one result will be returned.
								form1.Style.Remove("display");
								form1.Style.Add("display", "block");
								blogpost.InnerHtml = dr["body"].ToString();
								blogtitle.Text = dr["title"].ToString();
								DateTime postdate = (DateTime)dr["postdate"];
								postyear.Text = postdate.Year.ToString();
								postday.Text = postdate.Day.ToString();
								postmonth.Text = postdate.ToString("MMMM");
								hiddenPostID.Value = postID.ToString();


							}
						}
					}
				}
			}
			else
			{
				// New post.
				postyear.Text = DateTime.Now.Year.ToString();
				postday.Text = DateTime.Now.Day.ToString();
				postmonth.Text = DateTime.Now.ToString("MMMM");
			}
		}


		protected int GetPostIDFromQueryString()
		{

			NameValueCollection parameters = Request.QueryString;

			string[] keys = parameters.AllKeys;

			int postID = 0;
			foreach (string key in keys)
			{
				if (key.ToUpperInvariant() == "P")
				{
					foreach (string value in parameters.GetValues(key))
					{
						if (System.Int32.TryParse(value, out postID))
						{
							break;
						}
					}
				}
			}
			return postID;
		}

		private DateTime GetPostDate()
		{
			DateTime dt = DateTime.MinValue;

			if (String.IsNullOrEmpty(postday.Text) || String.IsNullOrEmpty(postmonth.Text) || String.IsNullOrEmpty(postyear.Text))
				return dt;

			string postdate = postday.Text + " " + postmonth.Text + " " + postyear.Text;

			DateTime.TryParse(postdate, out dt);
			return dt;

		}

		private string GetPostDateErrors()
		{

			if (String.IsNullOrEmpty(postday.Text) || String.IsNullOrEmpty(postmonth.Text) || String.IsNullOrEmpty(postyear.Text))
				return "The Day, Month and Year fields must all be entered";

			return "";

		}

		public bool SavePost()
		{


			Page.Validate();

			bool pageIsValid = Page.IsValid ;

			DateTime dt = GetPostDate();
			if (dt == DateTime.MinValue)
			{
				string errors = GetPostDateErrors();
				date_error.Text = errors;
				pageIsValid = false;
			}
			else
				date_error.Text = String.Empty;

			if (pageIsValid == false)
			{
				return false;
			}

			int postID;
			if (Int32.TryParse(hiddenPostID.Value, out postID) == false)
				postID = 0;

			string cs = ConfigurationManager.ConnectionStrings["default"].ConnectionString;

			using (SqlConnection connection = new SqlConnection(cs))
			{
				if (connection == null)
					return false;

				connection.Open();

				SqlCommand sc;

				if (postID == 0)
				{
					sc = new SqlCommand("CreatePost");
				}
				else
				{
					sc = new SqlCommand("EditPost");
					sc.Parameters.AddWithValue("PostID", postID);
				}

				sc.Parameters.AddWithValue("title", blogtitle.Text);
				sc.Parameters.AddWithValue("stub", GetStub(blogtitle.Text));
				sc.Parameters.AddWithValue("body", blogpost.InnerHtml);

				sc.Connection = connection;
				sc.CommandType = CommandType.StoredProcedure;

				int result = sc.ExecuteNonQuery();

				return (result > -2);

			}

		}

		string GetStub(string title)
		{
			string stub = title.Replace(" ", "-").ToLower();
			int suffix = 1;
			while (DoesStubAlreadyExist(stub) == true)
			{
				stub += suffix.ToString();
				suffix++;
			}

			return stub;


		}

		bool DoesStubAlreadyExist(string stub)
		{

			string cs = ConfigurationManager.ConnectionStrings["default"].ConnectionString;

			using (SqlConnection connection = new SqlConnection(cs))
			{
				if (connection != null)
				{

					connection.Open();
					SqlCommand sc = new SqlCommand();

					sc.Connection = connection;

					sc.CommandText = "DoesStubExist";
					sc.Parameters.AddWithValue("Stub", stub);
					sc.CommandType = CommandType.StoredProcedure;

					bool exists = ((int)sc.ExecuteScalar() != 0);

					return exists;
				}
				else
				{
					return true; // If can't determine, assume it exists
				}
			}

		}


	}

}


